''' <summary>
''' Programm zum Starten des Demo-CmisService
''' </summary>
''' <remarks>
''' Bitte als Administrator ausführen!
'''</remarks>
Module Module1

   Sub Main()
      Console.WriteLine(Environment.CommandLine)
      Console.WriteLine()

      Dim url As String = String.Format(Configuration.ConfigurationManager.AppSettings("url"), Environment.MachineName.ToLower())

      'Cmis-Service
      ' ' ' ' ' ' ' '
      Console.WriteLine("Cmis-Service")

      Dim cmisHost_AtomPub As New CmisObjectModel.ServiceModel.AtomPub.ServiceManager()
      Dim url_AomPub As String = url & "/atom"
      cmisHost_AtomPub.Open(New Uri(url_AomPub))
      Console.WriteLine(" - AtomPub: " & url_AomPub)

      'Dim cmisHost_Browser As New CmisObjectModel.ServiceModel.Browser.ServiceManager()
      'Dim url_Browser As String = url & "/browser"
      'cmisHost_Browser.Open(New Uri(url_Browser))
      'Console.WriteLine(" - Browser: " & url_Browser)

      Console.WriteLine()

      Console.WriteLine("Endpunkt-Beschreibung")
      Console.WriteLine(" - AtomPub: " & url_AomPub & "/help")
      'Console.WriteLine(" - Browser: " & url_Browser & "/help")

      Console.WriteLine()

      'Web-Service
      ' ' ' ' ' ' '
      Console.WriteLine("Web-Service (URL-Templates)")
      Dim url_Web As String = url & "extra"
      Dim webHost As New ServiceModel.ServiceHost(GetType(WebServer.WebService), New Uri(url_Web))
      webHost.AddServiceEndpoint(GetType(WebServer.IWebService), New ServiceModel.WebHttpBinding(), String.Empty)
      webHost.Description.Endpoints.Single.Behaviors.Add(New ServiceModel.Description.WebHttpBehavior())
      webHost.Open()
      Console.WriteLine(" - Übersicht: " & url_Web & "/obj?id={0}")
      Console.WriteLine(" - Metadaten: " & url_Web & "/meta?id={0}")
      Console.WriteLine(" - Download:  " & url_Web & "/file?id={0}")
      Console.WriteLine()


      Console.WriteLine("running...")
      Console.WriteLine("Ctrl+C for exit")
      Console.WriteLine()

      While True
         Threading.Thread.Sleep(100)

         While CmisServer.CmisServiceImpl.InMemoryLogQueue.Count > 0
            Dim text As String = CmisServer.CmisServiceImpl.InMemoryLogQueue.Dequeue
            If text.StartsWith("ERROR") Then
               WriteLineInColor(ConsoleColor.Red, text)
            ElseIf text.Contains("Check") OrElse text.Contains("Create") OrElse text.Contains("Delete") OrElse text.Contains("Properties") OrElse text.Contains("Content") Then
               WriteLineInColor(ConsoleColor.Yellow, text)
            Else
               Console.WriteLine(text)
            End If
         End While

      End While

   End Sub

   Sub WriteLineInColor(color As ConsoleColor, text As String)
      Dim oldColor As ConsoleColor = Console.ForegroundColor
      Console.ForegroundColor = color
      Console.WriteLine(text)
      Console.ForegroundColor = oldColor
   End Sub
End Module
