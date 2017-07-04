Imports CCL = CmisObjectModel.Client
Imports CCO = CmisObjectModel.Common
Imports CCP = CmisObjectModel.Core.Properties
Imports CCD = CmisObjectModel.Core.Definitions
Imports CMR = CmisObjectModel.Messaging.Requests

Module Module1

   Sub Main()

      Dim HOST As String = ReadLine("Host: ")
      If String.IsNullOrWhiteSpace(HOST) Then
         HOST = Environment.MachineName
      End If
      Console.CursorTop -= 1
      Console.WriteLine("Host:       " & HOST)

      Dim USER As String = "DemoClient"
      Dim PWD As String = "dEmOcLiEnT"
      Dim REPO As String = "repo"

      '1. Identität festlegen
      Dim authentication As New CCL.AuthenticationProvider(USER, ToSecureString(PWD))
      Console.WriteLine("User:       " & USER)

      Dim URL As String = "http://" & HOST & "/cmis/atom"
      Console.WriteLine("Service:    " & URL)

      '2. CMIS-Client erzeugen
      Dim client As CCL.Base.CmisClient = New CCL.AtomPub.CmisClient(New Uri(URL), CCO.enumVendor.other, authentication)
      Console.WriteLine("Binding:    AtomPub")

      '3. Mit dem Repository verbinden (LogIn)
      Dim repository As CCL.CmisRepository = (New CCL.CmisService(client)).GetRepositoryInfo(REPO)
      Console.WriteLine("Repository: " & repository.Description)

      '4. Anfragen stellen
      Try

         'TypeDefinition
         Console.WriteLine()
         Console.WriteLine("Types:")
         Dim reqest As New CMR.getTypeDefinition() With {.RepositoryId = REPO}
         For Each basetype As CCL.Generic.ItemContainer(Of CCL.CmisType) In repository.GetTypeDescendants()
            ShowType(basetype)
         Next

         Dim root_folder As CCL.CmisFolder = repository.GetRootFolder()

         'Properties
         Console.WriteLine()
         Console.WriteLine("Properties of root: " & root_folder.Properties.Properties.Count)
         Dim type As CCL.CmisType = repository.GetTypeDefinition(root_folder.ObjectTypeId)
         ShowProperties(root_folder, type)

         'Objects
         Console.WriteLine()
         Console.WriteLine("Folder structure")
         ShowStructure(root_folder)

      Catch ex As Exception
         Console.WriteLine(ex.Message)
         If repository.LastException IsNot Nothing Then
            Console.WriteLine(repository.LastException.Message)
         End If
      End Try

      '5. Verbindung zum Repository trennen
      client.Logout(REPO)

      Console.WriteLine(vbCrLf & New String("-"c, Console.WindowWidth - 1))
      ReadLine("End.")
   End Sub

   Function ReadLine(text As String)
      Console.Write(text)
      Return Console.ReadLine()
   End Function

   Function ToSecureString(str As String)
      Dim secureString As New Security.SecureString
      For Each c As Char In str.ToCharArray()
         secureString.AppendChar(c)
      Next
      Return secureString
   End Function

   Sub ShowType(type As CCL.Generic.ItemContainer(Of CCL.CmisType), Optional shift As Integer = 1)
      Console.WriteLine(New String(" "c, shift) & "- " & type.Item.Type.DisplayName & " (" & type.Item.Type.Id & ")")

      Console.WriteLine(New String(" "c, shift) & " - PropertyDefinitions: " & type.Item.Type.PropertyDefinitions.Count)
      For Each propDefinition In type.Item.Type.PropertyDefinitions
         Console.WriteLine(New String(" "c, shift) & "  - " & propDefinition.DisplayName & " (" & propDefinition.Id & "): " _
                           & String.Join(" ", propDefinition.Updatability, propDefinition.Cardinality, propDefinition.PropertyType))
      Next

      Console.WriteLine(New String(" "c, shift) & " - SubTypes: " & type.Count)
      For Each subType As CCL.Generic.ItemContainer(Of CCL.CmisType) In type
         ShowType(subType, shift + 1)
      Next
   End Sub

   Sub ShowProperties(obj As CCL.CmisObject, type As CCL.CmisType)
      For Each prop As CCP.cmisProperty In obj.Properties.Properties

         Console.Write(" - " & prop.PropertyDefinitionId & " = ")

         'Ob eine Property ein Einzelwert oder ein Array der Länge 1 darstellt, lässt sich aus dem CMIS-Objekt selbst nicht ableiten.
         'Diese Information wird in der Serverantwort für das Objekt in der Regel nicht mitgeliefert.
         'Deswegen wird hier die PropertyDefinition herangezogen, die in der TypeDefinition enthalten ist.
         Dim propDef As CCD.Properties.cmisPropertyDefinitionType = type.Type.GetPropertyDefinitions(prop.PropertyDefinitionId).Values.Single()

         If propDef.Cardinality = CmisObjectModel.Core.enumCardinality.single Then
            Console.WriteLine(ValueToDisplayString(prop.Value))
         Else
            If prop.Values Is Nothing Then
               Console.WriteLine("(notSet)")
            Else
               Console.WriteLine("[" & String.Join("; ", From value As Object In prop.Values
                                                         Select ValueToDisplayString(value)) & "]")
            End If
         End If
      Next

   End Sub

   Function ValueToDisplayString(value As Object) As String
      If value Is Nothing Then
         Return "(notSet)"
      ElseIf String.IsNullOrWhiteSpace(value.ToString()) Then
         Return "(emptyOrWhiteSpace)"
      Else
         Return value.ToString()
      End If
   End Function

   Sub ShowStructure(folder As CCL.CmisFolder, Optional shift As Integer = 1)
      Dim children As CCL.Generic.ItemList(Of CCL.CmisObject) = folder.GetChildren()
      If children IsNot Nothing Then
         For Each child As CCL.CmisObject In children.Items
            Console.WriteLine(New String(" "c, shift) & "- " & child.ObjectTypeId.Value & " " & child.ObjectId.Value & " (" & child.Name.Value & ")")
            If "cmis:folder".Equals(child.ObjectTypeId) Then
               ShowStructure(child, shift + 1)
            End If
         Next
      End If
   End Sub

End Module
