Namespace Updater
    Public Class UpdateDownloadProgressChangedEventArgs
        Inherits EventArgs

        Private _bytesDownloaded As Integer
        ReadOnly Property BytesDownloaded As Integer
            Get
                Return _bytesDownloaded
            End Get
        End Property

        Public Sub New(bytesDown As Integer)
            _bytesDownloaded = bytesDown
        End Sub
    End Class
End Namespace
