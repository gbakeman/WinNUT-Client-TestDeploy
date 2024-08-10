Imports WinNUT_Client_Common

Namespace Forms
    Public Class UpdateAvailableForm

        Public Sub New()
            InitializeComponent()

            Title.Text = UpdateController.LatestRelease.Name
            Icon = WinNUT.Icon
            TB_ChgLog.Text = UpdateController.LatestRelease.Body
        End Sub

        Private Sub VisitPageButton_Click(sender As Object, e As EventArgs) Handles VisitPageButton.Click
            Process.Start(UpdateController.LatestRelease.HtmlUrl)
        End Sub

        Private Sub Update_Btn_Click(sender As Object, e As EventArgs) Handles Update_Btn.Click
            DownloadProgressPanel.Visible = True
            AddHandler UpdateController.UpdateDownloadProgressChanged, AddressOf UpdateDownloadProgressChanged
            AddHandler UpdateController.UpdateDownloadCompleted, AddressOf UpdateDownloadCompleted
            UpdateController.BeginUpdateDownload()
        End Sub

        Private Sub UpdateDownloadProgressChanged(sender As Object, e As Updater.UpdateDownloadProgressChangedEventArgs)
            If UpdateController.LatestReleaseAsset.Size > 0 Then
                DownloadProgressBar.Value = (e.BytesDownloaded / UpdateController.LatestReleaseAsset.Size) * 100
                DownloadProgressBar.Text = String.Format("{0:F2} MB / {1:F2} MB", e.BytesDownloaded / 1048576,
                                                         UpdateController.LatestReleaseAsset.Size / 1048576)
            End If
        End Sub

        Private Sub UpdateDownloadCompleted(sender As Object, e As Updater.UpdateDownloadCompletedEventArgs)
            Process.Start(e.DownloadedFile.FullName)
            Application.Exit()
        End Sub

        Private Sub Close_Btn_Click(sender As Object, e As EventArgs) Handles Close_Btn.Click
            Close()
        End Sub
    End Class
End Namespace
