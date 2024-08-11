Imports System.IO
Imports System.Net.Http
Imports Octokit

Namespace Updater
    Public Class UpdateUtil
        Private Const REPOSITORY_OWNER = "nutdotnet"
        Private Const REPOSITORY_NAME = "WinNUT-Client"
        Private Shared ReadOnly USER_AGENT_HEADER As String = $"{REPOSITORY_OWNER}"
        Private Shared ReadOnly PROGRESS_CHANGED_DELAY = 500 ' Milliseconds to delay between raising progress update events.

        Private _latestRelease As Release
        Private releaseAsset As ReleaseAsset

        Public Event UpdateCheckCompleted(sender As UpdateUtil, e As UpdateCheckCompletedEventArgs)
        Public Event UpdateDownloadProgressChanged(sender As UpdateUtil, e As UpdateDownloadProgressChangedEventArgs)
        Public Event UpdateDownloadCompleted(sender As UpdateUtil, e As UpdateDownloadCompletedEventArgs)

        Public Property LatestRelease As Release
            Get
                Return _latestRelease
            End Get
            Private Set(value As Release)
                _latestRelease = value
                releaseAsset = value.Assets.First(Function(asset As ReleaseAsset) asset.Name.ToLowerInvariant().EndsWith(".msi"))
            End Set
        End Property

        Public Property LatestReleaseAsset As ReleaseAsset
            Get
                Return releaseAsset
            End Get
            Private Set(value As ReleaseAsset)
                releaseAsset = value
            End Set
        End Property

        Public Async Function BeginUpdateCheck(acceptPreRelease As Boolean) As Task
            Try
                Dim releases = Await New GitHubClient(New ProductHeaderValue(USER_AGENT_HEADER)).Repository.Release.GetAll(
                REPOSITORY_OWNER, REPOSITORY_NAME)

                For Each rel As Release In releases
                    If (acceptPreRelease AndAlso rel.Prerelease) OrElse Not rel.Prerelease Then
                        LatestRelease = rel
                        RaiseEvent UpdateCheckCompleted(Me, New UpdateCheckCompletedEventArgs(rel))
                        Return
                    End If
                Next
            Catch ex As Exception
                RaiseEvent UpdateCheckCompleted(Me, New UpdateCheckCompletedEventArgs(Nothing, ex))
                Return
            End Try
        End Function

        Public Async Function BeginUpdateDownload() As Task
            If releaseAsset Is Nothing Then
                Throw New InvalidOperationException("No release asset available to download.")
            End If

            Dim downloadFilePath = Path.GetTempPath() & releaseAsset.Name

            If File.Exists(downloadFilePath) Then
                Dim fileInfo = New FileInfo(downloadFilePath)
                If fileInfo.Length <> releaseAsset.Size Then
                    File.Delete(downloadFilePath)
                Else
                    RaiseEvent UpdateDownloadCompleted(Me, New UpdateDownloadCompletedEventArgs(fileInfo))
                    Return
                End If
            End If

            Try
                Using client As New HttpClient()
                    Dim response = Await client.GetAsync(New Uri(releaseAsset.BrowserDownloadUrl),
                                                         HttpCompletionOption.ResponseHeadersRead)
                    response.EnsureSuccessStatusCode()

                    Using fs As New FileStream(downloadFilePath, IO.FileMode.Create),
                            contentStream = Await response.Content.ReadAsStreamAsync()

                        Dim buffer As Byte() = New Byte(4096) {}
                        Dim totalBytesRead As Integer
                        Dim nextProgressUpdate As Date
                        Dim bytesRead As Integer

                        Do
                            bytesRead = Await contentStream.ReadAsync(buffer, 0, buffer.Length)
                            Await fs.WriteAsync(buffer, 0, bytesRead)
                            totalBytesRead += bytesRead

                            If Date.Now >= nextProgressUpdate Then
                                RaiseEvent UpdateDownloadProgressChanged(Me,
                                                                New UpdateDownloadProgressChangedEventArgs(totalBytesRead))
                                nextProgressUpdate.AddTicks(PROGRESS_CHANGED_DELAY * 10)
                            End If
                        Loop While bytesRead > 0

                        RaiseEvent UpdateDownloadCompleted(Me,
                                        New UpdateDownloadCompletedEventArgs(New FileInfo(downloadFilePath)))
                    End Using
                End Using
            Catch ex As Exception
                RaiseEvent UpdateDownloadCompleted(Me, New UpdateDownloadCompletedEventArgs(Nothing, ex))
            End Try

        End Function

        ''' <summary>
        ''' Determine if the interval between update checks has exceeded the user's settings.
        ''' </summary>
        ''' <param name="autoCheckDelay">The int used to represent the <see cref="DateInterval"/> between updates.</param>
        ''' <param name="lastChecked">The date when updates were last checked.</param>
        ''' <returns>True if an update check is warranted based on the user's settings.</returns>
        Public Shared Function UpdateCheckDelayPassed(autoCheckDelay As Integer, lastChecked As Date) As Boolean
            Dim DelayVerif As DateInterval
            Select Case autoCheckDelay
                Case 0
                    DelayVerif = DateInterval.Day
                Case 1
                    DelayVerif = DateInterval.Weekday
                Case 2
                    DelayVerif = DateInterval.Month
            End Select

            Dim Diff = 1

            If lastChecked <> Date.MinValue Then
                Diff = DateDiff(DelayVerif, lastChecked, Now)
            End If

            Return Diff >= 1
        End Function
    End Class
End Namespace
