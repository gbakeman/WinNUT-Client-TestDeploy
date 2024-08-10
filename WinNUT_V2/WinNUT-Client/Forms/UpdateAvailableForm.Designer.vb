Namespace Forms

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class UpdateAvailableForm
        Inherits System.Windows.Forms.Form

        'Form remplace la méthode Dispose pour nettoyer la liste des composants.
        <System.Diagnostics.DebuggerNonUserCode()>
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                If disposing AndAlso components IsNot Nothing Then
                    components.Dispose()
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

        'Requise par le Concepteur Windows Form
        Private components As System.ComponentModel.IContainer

        'REMARQUE : la procédure suivante est requise par le Concepteur Windows Form
        'Elle peut être modifiée à l'aide du Concepteur Windows Form.  
        'Ne la modifiez pas à l'aide de l'éditeur de code.
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(UpdateAvailableForm))
            Me.GB1 = New System.Windows.Forms.GroupBox()
            Me.Title = New System.Windows.Forms.Label()
            Me.TB_ChgLog = New System.Windows.Forms.TextBox()
            Me.Update_Btn = New System.Windows.Forms.Button()
            Me.Close_Btn = New System.Windows.Forms.Button()
            Me.ButtonsPanel = New System.Windows.Forms.Panel()
            Me.VisitPageButton = New System.Windows.Forms.Button()
            Me.DownloadProgressPanel = New System.Windows.Forms.Panel()
            Me.DownloadProgressPanelLabel = New System.Windows.Forms.Label()
            Me.DownloadProgressBar = New WinNUT_Client.WinFormControls.CProgressBar()
            Me.GB1.SuspendLayout()
            Me.ButtonsPanel.SuspendLayout()
            Me.DownloadProgressPanel.SuspendLayout()
            Me.SuspendLayout()
            '
            'GB1
            '
            resources.ApplyResources(Me.GB1, "GB1")
            Me.GB1.Controls.Add(Me.Title)
            Me.GB1.Controls.Add(Me.TB_ChgLog)
            Me.GB1.Name = "GB1"
            Me.GB1.TabStop = False
            '
            'Title
            '
            resources.ApplyResources(Me.Title, "Title")
            Me.Title.Name = "Title"
            '
            'TB_ChgLog
            '
            resources.ApplyResources(Me.TB_ChgLog, "TB_ChgLog")
            Me.TB_ChgLog.Name = "TB_ChgLog"
            '
            'Update_Btn
            '
            resources.ApplyResources(Me.Update_Btn, "Update_Btn")
            Me.Update_Btn.Name = "Update_Btn"
            Me.Update_Btn.UseVisualStyleBackColor = True
            '
            'Close_Btn
            '
            resources.ApplyResources(Me.Close_Btn, "Close_Btn")
            Me.Close_Btn.Name = "Close_Btn"
            Me.Close_Btn.UseVisualStyleBackColor = True
            '
            'ButtonsPanel
            '
            resources.ApplyResources(Me.ButtonsPanel, "ButtonsPanel")
            Me.ButtonsPanel.Controls.Add(Me.VisitPageButton)
            Me.ButtonsPanel.Controls.Add(Me.Update_Btn)
            Me.ButtonsPanel.Controls.Add(Me.Close_Btn)
            Me.ButtonsPanel.Name = "ButtonsPanel"
            '
            'VisitPageButton
            '
            resources.ApplyResources(Me.VisitPageButton, "VisitPageButton")
            Me.VisitPageButton.Name = "VisitPageButton"
            Me.VisitPageButton.UseVisualStyleBackColor = True
            '
            'DownloadProgressPanel
            '
            resources.ApplyResources(Me.DownloadProgressPanel, "DownloadProgressPanel")
            Me.DownloadProgressPanel.Controls.Add(Me.DownloadProgressPanelLabel)
            Me.DownloadProgressPanel.Controls.Add(Me.DownloadProgressBar)
            Me.DownloadProgressPanel.Name = "DownloadProgressPanel"
            '
            'DownloadProgressPanelLabel
            '
            resources.ApplyResources(Me.DownloadProgressPanelLabel, "DownloadProgressPanelLabel")
            Me.DownloadProgressPanelLabel.Name = "DownloadProgressPanelLabel"
            '
            'DownloadProgressBar
            '
            Me.DownloadProgressBar.ForeColor = System.Drawing.SystemColors.HighlightText
            resources.ApplyResources(Me.DownloadProgressBar, "DownloadProgressBar")
            Me.DownloadProgressBar.Name = "DownloadProgressBar"
            '
            'UpdateAvailableForm
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.Controls.Add(Me.GB1)
            Me.Controls.Add(Me.DownloadProgressPanel)
            Me.Controls.Add(Me.ButtonsPanel)
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.Name = "UpdateAvailableForm"
            Me.GB1.ResumeLayout(False)
            Me.GB1.PerformLayout()
            Me.ButtonsPanel.ResumeLayout(False)
            Me.DownloadProgressPanel.ResumeLayout(False)
            Me.DownloadProgressPanel.PerformLayout()
            Me.ResumeLayout(False)

        End Sub

        Friend WithEvents GB1 As GroupBox
        Friend WithEvents Close_Btn As Button
        Friend WithEvents Update_Btn As Button
        Friend WithEvents TB_ChgLog As TextBox
        Private WithEvents Title As Label
        Private WithEvents ButtonsPanel As Panel
        Friend WithEvents DownloadProgressPanel As Panel
        Friend WithEvents DownloadProgressPanelLabel As Label
        Friend WithEvents VisitPageButton As Button
        Friend WithEvents DownloadProgressBar As WinFormControls.CProgressBar
    End Class

End Namespace
