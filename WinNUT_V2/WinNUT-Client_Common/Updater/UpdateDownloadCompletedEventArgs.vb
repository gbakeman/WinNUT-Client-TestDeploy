' WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.
' Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
'
' This program is free software: you can redistribute it and/or modify it under the terms of the
' GNU General Public License as published by the Free Software Foundation, either version 3 of the
' License, or any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

Imports System.ComponentModel
Imports System.IO

Namespace Updater
    ''' <summary>
    ''' Represent important details resulting from a completed async request to download an update.
    ''' </summary>
    Public Class UpdateDownloadCompletedEventArgs
        Inherits AsyncCompletedEventArgs

        Private ReadOnly _downloadedFile As FileInfo

        ReadOnly Property DownloadedFile As FileInfo
            Get
                Return _downloadedFile
            End Get
        End Property

        Public Sub New(downloadedFile As FileInfo, Optional [error] As Exception = Nothing,
                       Optional cancelled As Boolean = False)

            MyBase.New([error], cancelled, Nothing)
            _downloadedFile = downloadedFile
        End Sub
    End Class
End Namespace
