Public Module WinNUT_Globals

#Region "Constants/Shareds"

    Public ReadOnly ProgramName As String = My.Application.Info.ProductName
    Public ReadOnly ProgramVersion As String = My.Application.Info.Version.ToString()
    Public ReadOnly ShortProgramVersion As String =
        ProgramVersion.Substring(0, ProgramVersion.IndexOf(".", ProgramVersion.IndexOf(".") + 1))
    Public ReadOnly GitHubURL As String = My.Application.Info.Trademark
    Public ReadOnly Copyright As String = My.Application.Info.Copyright
    Public ReadOnly DataDirectory = $"{ My.Application.Info.CompanyName }\{ ProgramName }\{ ProgramVersion }"

    Public WithEvents LogFile As Logger = New Logger(LogLvl.LOG_DEBUG)
    Public WithEvents UpdateController As New Updater.UpdateUtil
    Public StrLog As New List(Of String)

#End Region

    Public Sub Init_Globals()

    End Sub
End Module
