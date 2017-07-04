'Diese Datei enthält alle Funktionen, die intern aufgerufen werden.

Imports CmisObjectModel.Core
Imports CmisObjectModel.Messaging

Partial Public Class CmisServiceImpl

   Shared _repourl As String = Configuration.ConfigurationManager.AppSettings("url")
   Shared _repoid As String = Configuration.ConfigurationManager.AppSettings("repoid")
   Shared _reponame As String = Configuration.ConfigurationManager.AppSettings("reponame")
   Shared _folder As String = Configuration.ConfigurationManager.AppSettings("folder")
   Shared _errorfile As String = Configuration.ConfigurationManager.AppSettings("errorfile")

   Shared _repository As cmisRepositoryInfoType = Nothing

   Public Shared InMemoryLogQueue As New Queue(Of String)

   Public Sub New(url As Uri)
      MyBase.New(url)

      If Not IO.Directory.Exists(_folder) Then
         IO.Directory.CreateDirectory(_folder)
      End If
   End Sub

#Region "Logging and Errors"

   Sub Log_Internal(ParamArray values As String())
      InMemoryLogQueue.Enqueue("LOG   | " & Format(values))
   End Sub

   Sub ErrorLog_Internal(ParamArray values As String())
      Dim text As String = Format(values)
      InMemoryLogQueue.Enqueue("ERROR | " & text)
      If Not text.Contains("Not Found") Then
         IO.File.AppendAllText(_errorfile, text & vbCrLf)
      End If
   End Sub

   Function Format(ParamArray values As String()) As String
      For i As Integer = 0 To values.Length - 1
         If values(i) Is Nothing Then
            values(i) = "(null)"
         End If
         values(i) = values(i).ToString().PadRight(18)
      Next

      Return DateTime.Now.ToString() & " | " & String.Join(" | ", values)
   End Function

   Function NotSupported_Internal(method As String) As ServiceModel.Web.WebFaultException(Of cmisFaultType)
      Return cmisFaultType.CreateNotSupportedException("CmisServerDemo_" & method)
   End Function

#End Region

#Region "Identity"

   ReadOnly Property SystemAuthor_Internal As ServiceModel.Syndication.SyndicationPerson
      Get
         Return New ServiceModel.Syndication.SyndicationPerson("demo@cmis.bsw", "CmisServer Demo", "http://demo.bsw/cmis")
      End Get
   End Property

#End Region

#Region "TypeDefinition"

   ReadOnly Property TypeDefinition_Internal(typeId As String) As Definitions.Types.cmisTypeDefinitionType
      Get
         Dim filename As String = FindXmlPath(typeId.Replace(":", "_") & ".xml")
         Dim reader As Xml.XmlReader = Xml.XmlReader.Create(filename)
         Dim td As Definitions.Types.cmisTypeDefinitionType = Definitions.Types.cmisTypeDefinitionType.CreateInstance(reader)
         Return td
      End Get
   End Property

#End Region

#Region "Navigation"

   ReadOnly Property ParentFolderId_Internal(objectId As String) As String
      Get
         Dim parentFolderId As String = Nothing
         If objectId.Contains("\") Then
            Dim pos As Integer = objectId.LastIndexOf("\"c)
            parentFolderId = objectId.Substring(0, pos)
         Else
            parentFolderId = "root"
         End If

         Return parentFolderId
      End Get
   End Property

#End Region

#Region "Object"

   ReadOnly Property Exists_Internal(objectId As String) As Boolean
      Get
         If String.IsNullOrEmpty(objectId) Then
            Return False
         End If

         Dim pos As Integer = objectId.LastIndexOf(";")
         Dim versionSeriesId As String = If(pos > 0, objectId.Substring(0, pos), objectId)
         Dim isPwc As Boolean = objectId.EndsWith(";pwc")
         Dim isVersion As Boolean = Not isPwc AndAlso objectId.Contains(";") AndAlso pos < objectId.LastIndexOf(".")
         Dim path As String = System.IO.Path.Combine(_folder, versionSeriesId)
         If isPwc Then
            path = IO.Path.Combine(path, "pwc")
            Return IO.File.Exists(path)
         ElseIf isVersion Then
            path = IO.Path.Combine(path, "Versionen", objectId.Substring(pos + 1))
            Return IO.File.Exists(path)
         End If
         Dim versionExists As Boolean = IO.Directory.Exists(IO.Path.Combine(path, "Versionen"))
         Dim pwcExists As Boolean = IO.File.Exists(IO.Path.Combine(path, "pwc"))
         Return IO.Directory.Exists(path) AndAlso (versionExists OrElse (Not pwcExists AndAlso Not versionExists))
      End Get
   End Property

   ReadOnly Property Object_Internal(objectId As String) As CmisObjectModel.ServiceModel.cmisObjectType
      Get
         If Not "root".Equals(objectId) AndAlso Not Exists_Internal(objectId) Then
            Throw cmisFaultType.CreateNotFoundException(objectId)
         End If

         'Objekt-Art bestimmen
         Dim objTyp As String = Nothing
         Dim xmlTemplate As String = Nothing
         If "root".Equals(objectId) Then

            objTyp = "Stammverzeichnis"
            xmlTemplate = "folder"

         ElseIf objectId.EndsWith(";pwc") Then

            objTyp = "Arbeitskopie"
            xmlTemplate = "document"

         ElseIf objectId.Contains(";") AndAlso objectId.LastIndexOf(";") < objectId.LastIndexOf(".") Then

            objTyp = "Version"
            xmlTemplate = "document"

         ElseIf IO.Directory.Exists(IO.Path.Combine(_folder, objectId, "Versionen")) Then

            objTyp = "Dokument"
            xmlTemplate = "document"

         Else

            objTyp = "Ordner"
            xmlTemplate = "folder"

         End If

         Dim serializer As New Xml.Serialization.XmlSerializer(GetType(CmisObjectModel.ServiceModel.cmisObjectType))
         Dim xml As String = IO.File.ReadAllText(FindXmlPath(xmlTemplate & ".xml"))
         Dim obj As CmisObjectModel.ServiceModel.cmisObjectType = CType(serializer.Deserialize(New IO.StringReader(xml)), CmisObjectModel.ServiceModel.cmisObjectType)

         If "Stammverzeichnis".Equals(objTyp) Then

            Dim info As New IO.DirectoryInfo(_folder)

            'I. Grunddaten
            obj.Name = "Root"
            obj.ObjectId = "root"
            obj.Description = "Stammverzeichnis " & _folder

            'III. Änderungsdaten
            obj.CreationDate = info.CreationTime.ToUniversalTime()
            obj.LastModificationDate = info.LastWriteTime.ToUniversalTime()

            'VII. Ordner
            obj.Path = "/"

            'VIII. Change Token
            obj.ChangeToken = info.LastWriteTime.ToUniversalTime()

            'Erlaubte Aktionen anpassen
            obj.AllowableActions.CanDeleteObject = False

         ElseIf "Ordner".Equals(objTyp) Then

            Dim info As New IO.DirectoryInfo(IO.Path.Combine(_folder, objectId))
            Dim path As String = "/" & objectId.Replace("\", "/")
            Dim name As String = objectId.Split("\"c).Last()

            'I. Grunddaten
            obj.Name = name
            obj.ObjectId = objectId
            obj.Description = "Lokales Verzeichnis"

            'III. Änderungsdaten
            obj.CreationDate = info.CreationTime.ToUniversalTime()
            obj.LastModificationDate = info.LastWriteTime.ToUniversalTime()

            'VII. Ordner
            obj.Path = path
            obj.ParentId = ParentFolderId_Internal(objectId)

            'VIII. Change Token
            obj.ChangeToken = info.LastWriteTime.ToUniversalTime()

         Else

            Dim pos As Integer = objectId.LastIndexOf(";")
            Dim isLatest As Boolean = pos = -1
            Dim versionSeriesId As String = If(isLatest, objectId, objectId.Substring(0, pos))
            Dim name As String = versionSeriesId.Split("\"c).Last()
            Dim meta As Metadata = DocumentMetadata_Internal(versionSeriesId)
            Dim version As String = If(isLatest, meta.LabelOfLatestVersion, objectId.Substring(pos + 1))
            Dim isPwc As Boolean = "pwc".Equals(version)
            Dim visiblePwc As Boolean = meta.IsVersionSeriesCheckedOut AndAlso meta.VersionSeriesCheckedOutBy.Equals(CurrentAuthenticationInfo.User)
            Dim info As New IO.FileInfo(IO.Path.Combine(_folder, versionSeriesId, If(isPwc, String.Empty, "Versionen"), version))
            Dim isMajor As String = Not isPwc AndAlso version.EndsWith(".0")
            Dim isLatestMajor As Boolean = Not isPwc AndAlso isMajor AndAlso version.StartsWith(meta.MajorOfLatestVersion)

            'I. Grunddaten
            obj.Name = name
            obj.ObjectId = objectId
            obj.Description = If(isPwc, meta.DescriptionPwc, meta.Description)
            obj.Properties.GetProperties("patorg:akte").First.Value.Values = If(isPwc, meta.AktePwc, meta.Akte)

            'III. Änderungsdaten
            obj.CreatedBy = meta.CreatedBy
            obj.CreationDate = meta.CreationDate.ToUniversalTime()
            obj.LastModifiedBy = If(isLatest OrElse isPwc, meta.LastModifiedBy, "- nicht geseichert -")
            obj.LastModificationDate = If(isLatest OrElse isPwc, meta.LastModificationDate, info.LastWriteTime).ToUniversalTime()

            'IV. Versionsinfo
            obj.IsPrivateWorkingCopy = isPwc
            obj.IsLatestVersion = isLatest
            obj.IsMajorVersion = isMajor
            obj.IsLatestMajorVersion = isLatestMajor
            obj.VersionLabel = If(isLatest, meta.LabelOfLatestVersion, version)
            obj.VersionSeriesId = versionSeriesId

            'V. Versionierung
            If meta.IsVersionSeriesCheckedOut Then
               obj.IsVersionSeriesCheckedOut = True
               obj.VersionSeriesCheckedOutBy = meta.VersionSeriesCheckedOutBy
               If CurrentAuthenticationInfo.User.Equals(meta.VersionSeriesCheckedOutBy) Then
                  obj.VersionSeriesCheckedOutId = versionSeriesId & ";pwc"
               End If
            Else
               obj.IsVersionSeriesCheckedOut = False
            End If
            obj.CheckinComment = meta.GetComment(version)

            'VI. Datei
            obj.ContentStreamLength = info.Length
            obj.ContentStreamMimeType = meta.MimeType
            obj.ContentStreamFileName = name
            obj.ContentStreamId = objectId

            'VIII. Change Token
            obj.ChangeToken = info.LastWriteTime

            'Erlaubte Aktionen anpassen
            If "Arbeitskopie".Equals(objTyp) Then
               obj.AllowableActions.CanDeleteObject = True
               obj.AllowableActions.CanUpdateProperties = True
               obj.AllowableActions.CanSetContentStream = True
               obj.AllowableActions.CanCancelCheckOut = True
               obj.AllowableActions.CanCheckIn = True
            ElseIf "Dokument".Equals(objTyp) Then
               If Not meta.IsVersionSeriesCheckedOut Then
                  obj.AllowableActions.CanDeleteObject = True
                  obj.AllowableActions.CanCheckOut = True
                  obj.AllowableActions.CanUpdateProperties = True
               End If

            End If

         End If

         CompleteObject(obj)

         Return obj
      End Get
   End Property

   Function GetAllVersions_Internal(objectId As String) As CmisObjectModel.ServiceModel.cmisObjectListType
      Log_Internal("GetAllVersions", objectId)

      Dim list As New List(Of CmisObjectModel.ServiceModel.cmisObjectType)()

      Dim pos As Integer = objectId.LastIndexOf(";")
      Dim versionSeriesId As String = If(pos > 0, objectId.Substring(0, pos), objectId)

      For Each file As String In IO.Directory.EnumerateFiles(IO.Path.Combine(_folder, versionSeriesId, "Versionen"))
         list.Add(Object_Internal(versionSeriesId & ";" & file.Split("\"c).Last))
      Next

      list.RemoveAt(list.Count - 1)
      Dim obj As CmisObjectModel.ServiceModel.cmisObjectType = Object_Internal(versionSeriesId)
      list.Add(obj)
      If obj.IsVersionSeriesCheckedOut AndAlso obj.VersionSeriesCheckedOutBy.Equals(CurrentAuthenticationInfo.User) Then
         list.Add(Object_Internal(obj.VersionSeriesCheckedOutId))

#If WorkbenchTest = "True" Then
         'Damit die Workbench für "Check specification compliance" ein positives Ergebnis liefert,
         'wird hier vom CMIS-Standard abgewichen und die Arbeitskopie bekommt cmis:latestVersion = True.
         'Siehe CMIS-Standard 1.1 - Abschnitt 2.1.13.5.1 "Checkout".
         '(Diese Kompilerkonstante wird im Projekt CmisServer definiert.)

         obj.IsLatestVersion = False
         list.Last.IsLatestVersion = True
#End If

      End If

      list.Reverse()

      Return New CmisObjectModel.ServiceModel.cmisObjectListType With {.Objects = list.ToArray()}
   End Function

   Private Sub CompleteObject(obj As CmisObjectModel.ServiceModel.cmisObjectType)
      If BaseUri.ToString.ToLower.Contains("atom") Then
         'Für die AtomPub-Binding müssen Author und ContentLink eines CMIS-Objekt selbst gesetzt werden

         obj.ServiceModel.Author = SystemAuthor_Internal
         If "cmis:document".Equals(obj.BaseTypeId) Then
            obj.ServiceModel.ContentLink = New CmisObjectModel.AtomPub.AtomLink(New Uri(String.Format("{0}{1}/content?objectId={2}", BaseUri, _repoid, obj.ObjectId)))
         End If
      End If
   End Sub

   Property DocumentMetadata_Internal(objectId As String) As Metadata
      Get
         Dim pos As Integer = objectId.LastIndexOf(";")
         Dim versionSeriesId As String = If(pos > 0, objectId.Substring(0, pos), objectId)

         Dim path As String = IO.Path.Combine(_folder, versionSeriesId, "metadata")
         Dim xml As String = IO.File.ReadAllText(path)
         Dim meta As Metadata = Metadata.FromXml(xml)

         Return meta
      End Get
      Set(value As Metadata)
         Dim path As String = IO.Path.Combine(_folder, objectId.Replace(";pwc", String.Empty), "metadata")
         Dim xml As String = value.ToXml()

         IO.File.WriteAllText(path, xml)
      End Set
   End Property

#End Region

#Region "Properties"

   Sub UpdateProperties_Internal(objectId As String, properties As Collections.cmisPropertiesType, changeToken As String)

      Dim meta As Metadata = DocumentMetadata_Internal(objectId)

      If objectId.EndsWith(";pwc") Then
         If properties.GetProperties("cmis:description").Count > 0 Then
            meta.DescriptionPwc = properties.GetProperties("cmis:description").Values.First.Value
         End If
         If properties.GetProperties("patorg:akte").Count > 0 Then
            Dim prop As CmisObjectModel.Core.Properties.cmisProperty = properties.GetProperties("patorg:akte").Values.First
            If prop.Values Is Nothing Then
               meta.AktePwc = Nothing
            Else
               meta.AktePwc = (From obj As Object In prop.Values
                               Let str As String = obj.ToString()
                               Select str).ToArray()
            End If
         End If
         If properties.GetProperties("cmis:foreignChangeToken").Count > 0 Then
            meta.ForeignChangeToken = properties.GetProperties("cmis:foreignChangeToken").Values.First.Value
         End If
      Else
         If properties.GetProperties("cmis:description").Count > 0 Then
            meta.Description = properties.GetProperties("cmis:description").Values.First.Value
         End If
         If properties.GetProperties("patorg:akte").Count > 0 Then
            Dim objs() As Object = properties.GetProperties("patorg:akte").Values.First.Values
            If objs Is Nothing Then
               meta.Akte = Nothing
            Else
               meta.Akte = (From obj As Object In properties.GetProperties("patorg:akte").Values.First.Values
                            Let str As String = obj.ToString()
                            Select str).ToArray()
            End If
         End If
      End If

      DocumentMetadata_Internal(objectId) = meta
   End Sub

#End Region

#Region "CheckOut/CheckIn"

   Function CheckOut_Internal(objectId As String) As String
      Dim meta As Metadata = DocumentMetadata_Internal(objectId)

      If Not meta.IsVersionSeriesCheckedOut Then

         Dim pathOriginal As String = IO.Path.Combine(_folder, objectId, "Versionen", meta.LabelOfLatestVersion)
         Dim pathPwc As String = IO.Path.Combine(_folder, objectId, "pwc")

         IO.File.Copy(pathOriginal, pathPwc)

         meta.VersionSeriesCheckedOutBy = CurrentAuthenticationInfo.User
         meta.DescriptionPwc = meta.Description
         meta.AktePwc = meta.Akte

         DocumentMetadata_Internal(objectId) = meta

      ElseIf Not meta.VersionSeriesCheckedOutBy.Equals(CurrentAuthenticationInfo.User) Then
         Throw New Exception("'" & objectId & "' is checked out by '" & meta.VersionSeriesCheckedOutBy & "'!")
      End If

      Dim pwcId As String = objectId
      If Not pwcId.EndsWith(";pwc") Then
         pwcId &= ";pwc"
      End If

      Return pwcId
   End Function

   Function CheckIn_Internal(objectId As String, properties As Collections.cmisPropertiesType, policies() As String, content As cmisContentStreamType, major As Boolean, checkInComment As String, Optional addACEs As Security.cmisAccessControlListType = Nothing, Optional removeACEs As Security.cmisAccessControlListType = Nothing) As String

      Dim meta As Metadata = DocumentMetadata_Internal(objectId)

      If Not meta.IsVersionSeriesCheckedOut Then
         Throw New Exception("not checked out")
      End If

      If properties IsNot Nothing AndAlso properties.Count > 0 Then
         UpdateProperties_Internal(objectId, properties, Nothing)
         meta = DocumentMetadata_Internal(objectId)
      End If

      If content IsNot Nothing Then
         SetContentStream_Internal(objectId, content.BinaryStream, content.MimeType, content.Filename, True, Nothing)
      End If

      If meta.VersionSeriesCheckedOutBy.Equals(CurrentAuthenticationInfo.User) Then
         If major Then
            meta.MajorOfLatestVersion += 1
            meta.MinorOfLatestVersion = 0
         Else
            meta.MinorOfLatestVersion += 1
         End If

         meta.Description = meta.DescriptionPwc
         meta.DescriptionPwc = Nothing

         meta.Akte = meta.AktePwc
         meta.AktePwc = Nothing

         Dim comment As String = checkInComment
         If String.IsNullOrEmpty(comment) Then comment = meta.Description
         meta.AddComment(comment)

         Dim pathVersionen As String = IO.Path.Combine(_folder, objectId.Replace(";pwc", String.Empty), "Versionen")
         Dim pathNew As String = IO.Path.Combine(pathVersionen, meta.LabelOfLatestVersion)
         Dim pathPwc As String = IO.Path.Combine(_folder, objectId.Replace(";pwc", String.Empty), "pwc")

         If Not IO.Directory.Exists(pathVersionen) Then
            IO.Directory.CreateDirectory(pathVersionen)
         End If

         If "enumVersioningState.checkedout".Equals(meta.GetComment("0.1")) AndAlso IO.File.Exists(IO.Path.Combine(pathVersionen, "0.1")) Then
            IO.File.Delete(IO.Path.Combine(pathVersionen, "0.1"))
            meta.CheckinComments = New Metadata.CheckinComment() {meta.CheckinComments.Last}
         End If

         IO.File.Copy(pathPwc, pathNew)
         IO.File.Delete(pathPwc)

         meta.VersionSeriesCheckedOutBy = Nothing

         meta.LastModifiedBy = CurrentAuthenticationInfo.User
         meta.LastModificationDate = DateTime.Now.ToUniversalTime()

         DocumentMetadata_Internal(objectId) = meta

      Else
         Throw New Exception("'" & objectId & "' is checked out by '" & meta.VersionSeriesCheckedOutBy & "'!")
      End If

      Return objectId.Replace(";pwc", String.Empty)
   End Function

   Sub CancelCheckOut_Internal(objectId As String)
      Dim meta As Metadata = DocumentMetadata_Internal(objectId)

      If Not meta.VersionSeriesCheckedOutBy.Equals(CurrentAuthenticationInfo.User) Then
         Throw New Exception("'" & objectId & "' is checked out by '" & meta.VersionSeriesCheckedOutBy & "'!")
      End If

      Dim versionSeriesId As String = objectId.Replace(";pwc", String.Empty)
      Dim path As String = IO.Path.Combine(_folder, versionSeriesId)

      If "enumVersioningState.checkedout".Equals(meta.GetComment()) Then
         'Hier wird anhand des Kommentars erkannt, dass ein Dokument mit dem Status "checkedout" erzeugt wurde.
         'Mit dem CancelCheckOut muss dann hier auch die eingecheckte Version gelöscht werden.
         '(Siehe auch CmisService.CreateDocument)
         IO.File.Delete(path)
      Else

         Dim pathVersionen As String = IO.Path.Combine(_folder, versionSeriesId, "Versionen")
         If IO.Directory.Exists(pathVersionen) Then
            Dim pathPwc As String = IO.Path.Combine(_folder, versionSeriesId, "pwc")

            IO.File.Delete(pathPwc)

            meta.AktePwc = Nothing
            meta.DescriptionPwc = Nothing

            meta.VersionSeriesCheckedOutBy = Nothing
            DocumentMetadata_Internal(objectId) = meta
         Else
            IO.Directory.Delete(path, True)
         End If


      End If
   End Sub

#End Region

#Region "Content"

   Function SetContentStream_Internal(objectId As String, contentStream As IO.Stream, mimeType As String, fileName As String, overwriteFlag As Boolean, changeToken As String) As DateTime
      Dim path As String = IO.Path.Combine(_folder, objectId.Replace(";pwc", String.Empty), "pwc")

      Using fileStream As New IO.FileStream(path, IO.FileMode.Create)
         contentStream.CopyTo(fileStream)
      End Using

      Dim meta As Metadata = DocumentMetadata_Internal(objectId)
      meta.LastModificationDate = DateTime.Now.ToUniversalTime()
      If Not String.IsNullOrEmpty(mimeType) Then
         meta.MimeType = mimeType
      End If
      DocumentMetadata_Internal(objectId) = meta

      Return meta.LastModificationDate
   End Function

#End Region

End Class
