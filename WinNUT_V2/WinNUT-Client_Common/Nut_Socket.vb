' WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.
' Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
'
' This program is free software: you can redistribute it and/or modify it under the terms of the
' GNU General Public License as published by the Free Software Foundation, either version 3 of the
' License, or any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY



' Class dealing only with the management of the communication socket with the Nut server
Imports System.IO
Imports System.Net
Imports System.Net.Sockets

Public Class Nut_Socket

#Region "Properties"
    Public ReadOnly Property ConnectionStatus As Boolean
        Get
            Return client.Connected
        End Get
    End Property

    Public ReadOnly Property IsConnected() As Boolean
        Get
            Return ConnectionStatus
        End Get
    End Property

    Private _isLoggedIn As Boolean = False
    Public ReadOnly Property IsLoggedIn() As Boolean
        Get
            Return _isLoggedIn
        End Get
    End Property

    Private Nut_Ver As String
    Public ReadOnly Property Nut_Version() As String
        Get
            Return Nut_Ver
        End Get
    End Property

    Private Net_Ver As String
    Public ReadOnly Property Net_Version() As String
        Get
            Return Net_Ver
        End Get
    End Property
#End Region

    Private LogFile As Logger
    Private NutConfig As Nut_Parameter

    'Socket Variables
    Private client As New TcpClient
    Private NutStream As NetworkStream
    Private ReaderStream As StreamReader
    Private WriterStream As StreamWriter

    ''' <summary>
    ''' Possibly a race condition going on where a query is sent while reading a response from another.
    ''' </summary>
    Private streamInUse As Boolean

    Public Event Socket_Broken(ex As NutException)

    Public Sub New(Nut_Config As Nut_Parameter, ByRef logger As Logger)
        LogFile = logger
        NutConfig = Nut_Config
    End Sub

    Public Sub Connect()
        'TODO: Use LIST UPS protocol command to get valid UPSs.
        Dim Host = NutConfig.Host
        Dim Port = NutConfig.Port
        Dim Login = NutConfig.Login
        Dim Password = NutConfig.Password

        If String.IsNullOrEmpty(Host) Or IsNothing(Port) Then
            Throw New InvalidOperationException("Host and Port must be specified to connect.")
        End If

        Try
            LogFile.LogTracing(String.Format("Attempting TCP socket connection to {0}:{1}...", Host, Port), LogLvl.LOG_NOTICE, Me)

            client.Connect(Host, Port)
            NutStream = client.GetStream()
            ReaderStream = New StreamReader(NutStream)
            WriterStream = New StreamWriter(NutStream)

            LogFile.LogTracing("Connection established and streams ready.", LogLvl.LOG_NOTICE, Me)

        Catch Excep As Exception
            Disconnect(True)
            Throw ' Pass exception on up to UPS
        End Try

        Dim Nut_Query = Query_Data("VER")

        If Nut_Query.ResponseType = NUTResponse.OK Then
            Nut_Ver = (Nut_Query.RawResponse.Split(" "c))(4)
        End If
        Nut_Query = Query_Data("NETVER")

        If Nut_Query.ResponseType = NUTResponse.OK Then
            Net_Ver = Nut_Query.RawResponse
        End If

        LogFile.LogTracing(String.Format("NUT server reports VER: {0} NETVER: {1}", Nut_Ver, Net_Ver), LogLvl.LOG_NOTICE, Me)
    End Sub

    Public Sub Login()
        If _isLoggedIn Then
            Throw New InvalidOperationException("Attempted to login when already logged in.")
        End If

        LogFile.LogTracing(String.Format("Logging in to UPS [{0}] as user [{1}] ({2})...",
                            NutConfig.UPSName, NutConfig.Login,
                            If(String.IsNullOrEmpty(NutConfig.Password),
                                "NO Password", "Password provided")), LogLvl.LOG_NOTICE, Me)

        If Not String.IsNullOrEmpty(NutConfig.Login) Then
            Query_Data("USERNAME " & NutConfig.Login)

            If Not String.IsNullOrEmpty(NutConfig.Password) Then
                Query_Data("PASSWORD " & NutConfig.Password)
            End If
        End If

        Query_Data("LOGIN " & NutConfig.UPSName)
        _isLoggedIn = True
        LogFile.LogTracing("Authenticated successfully.", LogLvl.LOG_NOTICE, Me)
    End Sub

    ''' <summary>
    ''' Perform various functions necessary to disconnect the socket from the NUT server.
    ''' </summary>
    ''' <param name="forceful">Skip sending the LOGOUT command to the NUT server. Unknown effects.</param>
    Public Sub Disconnect(Optional forceful = False)
        If IsConnected Then
            If IsLoggedIn AndAlso Not forceful Then
                Query_Data("LOGOUT")
            End If

            If WriterStream IsNot Nothing Then
                WriterStream.Close()
            End If

            If ReaderStream IsNot Nothing Then
                ReaderStream.Close()
            End If

            If NutStream IsNot Nothing Then
                NutStream.Close()
            End If

            If client IsNot Nothing Then
                client.Close()
            End If
        Else
            Throw New InvalidOperationException("NUT Socket is already disconnected.")
        End If
    End Sub

    ''' <summary>
    ''' Parse and enumerate a NUT protocol response.
    ''' </summary>
    ''' <param name="Data">The raw response given from a query.</param>
    ''' <returns></returns>
    Private Function EnumResponse(Data As String) As NUTResponse
        Dim Response As NUTResponse
        ' Remove hyphens to prepare for parsing.
        Dim SanitisedString = UCase(Data.Replace("-", String.Empty))
        ' Break the response down so we can get specifics.
        Dim SplitString = SanitisedString.Split(" "c)

        Select Case SplitString(0)
            Case "OK", "VAR", "DESC", "UPS"
                Response = NUTResponse.OK
            Case "BEGIN"
                Response = NUTResponse.BEGINLIST
            Case "END"
                Response = NUTResponse.ENDLIST
            Case "ERR"
                Response = DirectCast([Enum].Parse(GetType(NUTResponse), SplitString(1)), NUTResponse)
            Case "NETWORK", "1.0", "1.1", "1.2"
                'In case of "VER" or "NETVER" Query
                Response = NUTResponse.OK
            Case Else
                ' We don't recognize the response, throw an error.
                Response = NUTResponse.NORESPONSE
                'Throw New Exception("Unknown response from NUT server: " & Response)
        End Select
        Return Response
    End Function

    ''' <summary>
    ''' Attempt to send a query to the NUT server, and do some basic parsing.
    ''' </summary>
    ''' <param name="Query_Msg">The query to be sent to the server, within specifications of the NUT protocol.</param>
    ''' <returns>The full <see cref="Transaction"/> of this function call.</returns>
    ''' <exception cref="InvalidOperationException">Thrown when calling this function while disconnected, or another
    ''' call is in progress.</exception>
    ''' <exception cref="NutException">Thrown when the NUT server returns an error or unexpected response.</exception>
    Function Query_Data(Query_Msg As String) As Transaction
        Dim Response As NUTResponse
        Dim DataResult As String
        Dim finalTransaction As Transaction

        If streamInUse Then
            Throw New InvalidOperationException("Attempted to query " & Query_Msg & " while stream is in use.")
        End If

        If ConnectionStatus Then
            streamInUse = True

            Try
                WriterStream.WriteLine(Query_Msg & vbCr)
                WriterStream.Flush()
            Catch
                Throw
            Finally
                streamInUse = False
            End Try

            DataResult = Trim(ReaderStream.ReadLine())
            Response = EnumResponse(DataResult)
            finalTransaction = New Transaction(Query_Msg, DataResult, Response)

            ' Handle error conditions
            If DataResult = Nothing OrElse DataResult.StartsWith("ERR") Then
                ' TODO: Does null dataresult really mean an error condition?
                ' https://stackoverflow.com/a/6523010/530172
                'Disconnect(True, True)
                'RaiseEvent Socket_Broken(New NutException(Query_Msg, Nothing))
                Throw New NutException(finalTransaction)
            End If
        Else
            Throw New InvalidOperationException("Attempted to send query while disconnected.")
        End If

        Return finalTransaction
    End Function

    Public Function Query_List_Datas(Query_Msg As String) As List(Of UPS_List_Datas)
        Dim List_Datas As New List(Of String)
        Dim List_Result As New List(Of UPS_List_Datas)
        Dim start As Date = Date.Now

        ' Read in first line to get initial response.
        ' LogFile.LogTracing("Sending LIST query " & Query_Msg, LogLvl.LOG_DEBUG, Me)
        Dim response = Query_Data(Query_Msg)
        streamInUse = True
        Dim readLine As String

        While True
            readLine = ReaderStream.ReadLine()

            If Not readLine.StartsWith("END") Then
                List_Datas.Add(readLine)
            Else
                Exit While
            End If
        End While

        streamInUse = False
        ' LogFile.LogTracing("Done processing LIST response for query " & Query_Msg, LogLvl.LOG_DEBUG, Me)

        Dim Key As String
        Dim Value As String
        For Each Line In List_Datas
            Dim SplitString = Split(Line, " ", 4)

            Select Case SplitString(0)
                Case "BEGIN"
                Case "VAR"
                    'Query 
                    'LIST VAR <upsname>
                    'Response List of var
                    'VAR <upsname><varname> "<value>"
                    Key = Replace(SplitString(2), """", "")
                    Value = Replace(SplitString(3), """", "")
                    Dim UPSName = SplitString(1)
                    Dim VarDESC = GetVarDescription(Key)
                    List_Result.Add(New UPS_List_Datas With {
                            .VarKey = Key,
                            .VarValue = Trim(Value),
                            .VarDesc = If(Not IsNothing(VarDESC), Split(Replace(VarDESC, """", ""), " ", 4)(3), String.Empty)}
                        )

                Case "UPS"
                    'Query 
                    'LIST UPS
                    'List of ups
                    'UPS <upsname> "<description>"
                    List_Result.Add(New UPS_List_Datas With {
                            .VarKey = "UPSNAME",
                            .VarValue = SplitString(1),
                            .VarDesc = Replace(SplitString(2), """", "")}
                        )
                Case "RW"
                    'Query 
                    'LIST RW <upsname>
                    'List of RW var
                    'RW <upsname><varname> "<value>"
                    Key = Replace(SplitString(2), """", "")
                    Value = Replace(SplitString(3), """", "")
                    Dim UPSName = SplitString(1)
                    Dim VarDESC = GetVarDescription(Key)
                    If Not IsNothing(VarDESC) Then
                        List_Result.Add(New UPS_List_Datas With {
                            .VarKey = Key,
                            .VarValue = Trim(Value),
                            .VarDesc = If(Not IsNothing(VarDESC), Split(Replace(VarDESC, """", ""), " ", 4)(3), String.Empty)}
                        )
                    Else
                        'TODO: Convert to nut_exception error
                        Throw New Exception("error")
                    End If
                Case "CMD"
                            'Query 
                            'LIST CMD <upsname>
                            'List of CMD
                            'CMD <upsname><cmdname>
                Case "ENUM"
                    'Query 
                    'LIST ENUM <upsname>
                    'List of Enum ??
                    'ENUM <upsname><varname> "<value>"
                    Key = Replace(SplitString(2), """", "")
                    Value = Replace(SplitString(3), """", "")
                    Dim UPSName = SplitString(1)
                    Dim VarDESC = Query_Data("GET DESC " & UPSName & " " & Key)
                    If VarDESC.ResponseType = NUTResponse.OK Then
                        List_Result.Add(New UPS_List_Datas With {
                            .VarKey = Key,
                            .VarValue = Value,
                            .VarDesc = Split(Replace(VarDESC.RawResponse, """", ""), " ", 4)(3)}
                        )
                    Else
                        'TODO: Convert to nut_exception error
                        Throw New Exception("error")
                    End If
                Case "RANGE"
                            'Query 
                            'LIST RANGE <upsname><varname>
                            'List of Range
                            'RANGE <upsname><varname> "<min>" "<max>"
                Case "CLIENT"
                    'Query 
                    'LIST CLIENT <upsname>
                    'List of Range
                    'CLIENT <device name><client IP address>
            End Select
        Next

        Return List_Result
    End Function

    Public Function GetVarDescription(VarName As String) As String
        Dim Nut_Query = Query_Data("GET DESC " & NutConfig.UPSName & " " & VarName)

        If Nut_Query.ResponseType = NUTResponse.OK Then
            Return Nut_Query.RawResponse
        Else
            Throw New NutException(Nut_Query)
        End If
    End Function

    Private Sub Event_WatchDog(sender As Object, e As EventArgs)
        Dim Nut_Query = Query_Data("")
        If Nut_Query.ResponseType = NUTResponse.NORESPONSE Then
            Disconnect(True)
            RaiseEvent Socket_Broken(New NutException(Nut_Query))
        End If
    End Sub
End Class
