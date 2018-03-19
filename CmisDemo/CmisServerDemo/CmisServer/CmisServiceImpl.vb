'Diese Datei enthält alle Funktionen, die vom CmisObjectModel aufgerufen werden.

Imports CmisObjectModel.Common.Generic
Imports CmisObjectModel.Core
Imports CmisObjectModel.Core.Collections
Imports CmisObjectModel.Core.Definitions.Types
Imports CmisObjectModel.Core.Security
Imports CmisObjectModel.Messaging
Imports CmisObjectModel.Messaging.Responses
Imports CmisObjectModel.RestAtom
Imports CmisObjectModel.ServiceModel

''' <summary>
''' Demo-Implementierung eines CmisServers
''' </summary>
''' <remarks>
''' CMIS-Standard 1.1 http://docs.oasis-open.org/cmis/CMIS/v1.1/os/CMIS-v1.1-os.html
''' </remarks>
Partial Public Class CmisServiceImpl
   Inherits CmisServiceImplBase

#Region "Logging and Errors"

   Protected Overrides Sub LogException(ex As Exception, method As Reflection.MethodBase)
      Dim cmisFault As ServiceModel.FaultException(Of cmisFaultType) = TryCast(ex, ServiceModel.FaultException(Of cmisFaultType))
      If cmisFault IsNot Nothing Then
         ErrorLog_Internal(method.Name, ex.Message, cmisFault.Detail.Message)
      ElseIf ex.InnerException IsNot Nothing Then
         ErrorLog_Internal(method.Name, ex.Message, ex.InnerException.Message)
      Else
         ErrorLog_Internal(method.Name, ex.Message, ex.GetType().ToString())
      End If
   End Sub

#End Region

#Region "Identity"

   Protected Overrides Function GetSystemAuthor() As ServiceModel.Syndication.SyndicationPerson
      Log_Internal("GetSystemAuthor")

      Return SystemAuthor_Internal
   End Function

   ''' <remarks>
   ''' Für die Browser-Binding ist die Validirung über diese Funktion deaktiviert. Bitte managen Sie 
   ''' Anmeldetokens und verwenden Sie die Transportsicherheit mittels SSL-Verschlüsselung.
   ''' Siehe: http://docs.oasis-open.org/cmis/CMIS/v1.1/errata01/os/CMIS-v1.1-errata01-os-complete.html#x1-5470009
   ''' </remarks>
   Public Overrides Function ValidateUserNamePassword(userName As String, password As String) As Boolean
      Log_Internal("ValidateUser", userName)

      Return userName.ToLower().Equals(password.ToLower()) OrElse "guest".Equals(userName)
   End Function

#End Region

#Region "Repository"

   Protected Overrides Function GetRepositories() As Result(Of cmisRepositoryInfoType())
      Log_Internal("GetRepositories")

      Return New cmisRepositoryInfoType() {RepositoryInfo(_repoid)}
   End Function

   Protected Overrides Function GetRepositoryInfo(repositoryId As String) As Result(Of cmisRepositoryInfoType)
      Log_Internal("GetRepositoryInfo", repositoryId)

      Return RepositoryInfo(repositoryId)
   End Function

   Protected Overrides ReadOnly Property RepositoryInfo(repositoryId As String) As cmisRepositoryInfoType
      Get
         If Not _repoid.Equals(repositoryId) Then
            Throw New Exception("Repository " & repositoryId & " not exists. Use " & _repoid)
         End If

         If _repository Is Nothing Then
            _repository = New cmisRepositoryInfoType

            _repository.RepositoryId = _repoid
            _repository.ProductName = "Demo CmisServicer"
            _repository.ProductVersion = "1.0"
            _repository.VendorName = "Brügmann Software GmbH"
            _repository.RepositoryName = _reponame
            _repository.RepositoryDescription = _reponame & " (" & _repoid & ")"
            _repository.RootFolderId = "root"
            _repository.CmisVersionSupported = "1.1"
            _repository.RepositoryUrl = BaseUri.ToString & _repoid

            _repository.PrincipalAnonymous = "guest"
            _repository.PrincipalAnyone = "GROUP_EVERYONE"

            _repository.Capabilities = New cmisRepositoryCapabilitiesType
            _repository.Capabilities.CapabilityPWCUpdatable = True

         End If

         Return _repository
      End Get
   End Property

#End Region

#Region "TypeDefinition"

   Protected Overrides Function GetTypeDefinition(repositoryId As String, typeId As String) As Result(Of cmisTypeDefinitionType)
      Log_Internal("GetTypeDefinition", typeId)

      Return TypeDefinition(repositoryId, typeId)
   End Function

   Protected Overrides ReadOnly Property TypeDefinition(repositoryId As String, typeId As String) As cmisTypeDefinitionType
      Get
         Return TypeDefinition_Internal(typeId)
      End Get
   End Property

   Protected Overrides Function GetTypeChildren(repositoryId As String, typeId As String, includePropertyDefinitions As Boolean, maxItems As Long?, skipCount As Long?) As Result(Of cmisTypeDefinitionListType)
      Log_Internal("GetTypeChildren", typeId)

      Dim list As New cmisTypeDefinitionListType
      list.NumItems = 0
      Return list
   End Function

   Protected Overrides Function GetTypeDescendants(repositoryId As String, typeId As String, includePropertyDefinitions As Boolean, depth As Long?) As Result(Of cmisTypeContainer)
      Log_Internal("GetTypeDescendants", typeId)

      If String.IsNullOrEmpty(typeId) Then
         Return New cmisTypeContainer With {.Children = New cmisTypeContainer() {
            New cmisTypeContainer With {.Type = TypeDefinition_Internal("cmis:folder")},
            New cmisTypeContainer With {.Type = TypeDefinition_Internal("cmis:document")}}}
      Else
         Return New cmisTypeContainer With {.Children = New cmisTypeContainer() {}}
      End If
   End Function

   Protected Overrides Function GetParentTypeId(repositoryId As String, typeId As String) As String
      Return Nothing
   End Function

#End Region

#Region "Navigation"

   Protected Overrides Function GetChildren(repositoryId As String, folderId As String, maxItems As Long?, skipCount As Long?, filter As String, includeAllowableActions As Boolean?, includeRelationships As enumIncludeRelationships?, renditionFilter As String, orderBy As String, includePathSegment As Boolean) As Result(Of CmisObjectModel.ServiceModel.cmisObjectInFolderListType)
      Log_Internal("GetChildren", folderId)

      Dim children As New List(Of CmisObjectModel.ServiceModel.cmisObjectInFolderType)

      Dim path As String = IO.Path.Combine(_folder, If("root".Equals(folderId), String.Empty, folderId))

      If IO.File.Exists(IO.Path.Combine(path, "metadata")) Then
         Return (cmisFaultType.CreateInvalidArgumentException("'" & folderId & "' is not a folder."))
      ElseIf IO.Directory.Exists(path) Then
         Dim folders As IEnumerable = IO.Directory.EnumerateDirectories(path)
         For Each folder As String In folders
            If IO.Directory.Exists(IO.Path.Combine(folder, "Versionen")) Then

               'Dokument

               Dim child As New CmisObjectModel.ServiceModel.cmisObjectInFolderType
               child.Object = Object_Internal(folder.Replace(_folder & "\", String.Empty))

               children.Add(child)
            End If

            If IO.File.Exists(IO.Path.Combine(folder, "pwc")) Then

               'Arbeitskopie

               Dim meta As Metadata = DocumentMetadata_Internal(folder.Replace(_folder & "\", String.Empty))
               If CurrentAuthenticationInfo.User.Equals(meta.VersionSeriesCheckedOutBy) Then
                  Dim child As New CmisObjectModel.ServiceModel.cmisObjectInFolderType
                  child.Object = Object_Internal(folder.Replace(_folder & "\", String.Empty) & ";pwc")

                  children.Add(child)
               End If

            End If

            If Not IO.Directory.Exists(IO.Path.Combine(folder, "Versionen")) AndAlso Not IO.File.Exists(IO.Path.Combine(folder, "pwc")) Then

               'Ordner

               Dim child As New CmisObjectModel.ServiceModel.cmisObjectInFolderType
               child.Object = Object_Internal(folder.Replace(_folder & "\", String.Empty))

               children.Add(child)
            End If
         Next
      End If

      Dim list As New CmisObjectModel.ServiceModel.cmisObjectInFolderListType()
      list.Objects = children.ToArray()
      list.NumItems = list.Count
      Return list
   End Function

   Protected Overrides Function GetFolderParent(repositoryId As String, folderId As String, filter As String) As Result(Of CmisObjectModel.ServiceModel.cmisObjectType)
      Log_Internal("GetFolderParent", folderId)

      Dim parentFolderId As String = ParentFolderId_Internal(folderId)
      Return Object_Internal(parentFolderId)
   End Function

   Protected Overrides Function GetObjectParents(repositoryId As String, objectId As String, filter As String, includeAllowableActions As Boolean?, includeRelationships As enumIncludeRelationships?, renditionFilter As String, includeRelativePathSegment As Boolean?) As Result(Of CmisObjectModel.ServiceModel.cmisObjectParentsType())
      Log_Internal("GetObjectParents", objectId)

      Dim parentFolderId As String = ParentFolderId_Internal(objectId)
      Dim parentFolder As New CmisObjectModel.ServiceModel.cmisObjectParentsType With {
            .Object = Object_Internal(parentFolderId),
            .RelativePathSegment = objectId.Split("\").Last}
      Return New CmisObjectModel.ServiceModel.cmisObjectParentsType() {parentFolder}
   End Function

#End Region

#Region "Object"

   Protected Overrides ReadOnly Property Exists(repositoryId As String, objectId As String) As Boolean
      Get
         Log_Internal("Exists", objectId)

         Return Exists_Internal(objectId)
      End Get
   End Property

   Protected Overrides Function GetObject(repositoryId As String, objectId As String, filter As String, includeRelationships As enumIncludeRelationships?, includePolicyIds As Boolean?, renditionFilter As String, includeACL As Boolean?, includeAllowableActions As Boolean?, returnVersion As enumReturnVersion?, privateWorkingCopy As Boolean?) As Result(Of CmisObjectModel.ServiceModel.cmisObjectType)
      Log_Internal("GetObject", objectId, returnVersion.ToString())

      Dim obj As CmisObjectModel.ServiceModel.cmisObjectType = Object_Internal(objectId)

      If returnVersion > enumReturnVersion.this Then
         'GetObjectOfLatesVersion

         Dim versions As CmisObjectModel.ServiceModel.cmisObjectListType = GetAllVersions_Internal(objectId)
         If returnVersion = enumReturnVersion.latestmajor Then
            'Latest major version

            Dim last = From version As CmisObjectModel.ServiceModel.cmisObjectType In versions.Objects
                       Where version.IsLatestMajorVersion AndAlso returnVersion = enumReturnVersion.latestmajor
                       Select version
            If last.Count > 0 Then obj = last.First() Else Throw New Exception("No Major Version")
         ElseIf returnVersion = enumReturnVersion.latest Then
            'Latest version

            obj = versions.First()
         End If
      End If

      Return obj
   End Function

   Protected Overrides Function GetObjectId(repositoryId As String, path As String) As String
      Log_Internal("GetObjectId", path)

      If "/".Equals(path) Then
         Return "root"
      Else
         Dim objectId As String = path.Substring(1).Replace("/", "\")

         If Not Exists_Internal(objectId) Then
            Throw New Exception("Object '" & objectId & "' not exists!")
         End If

         Return objectId
      End If
   End Function

   Protected Overrides Function GetObjectByPath(repositoryId As String, path As String, filter As String, includeAllowableActions As Boolean?, includePolicyIds As Boolean?, includeRelationships As enumIncludeRelationships?, includeACL As Boolean?, renditionFilter As String) As Result(Of CmisObjectModel.ServiceModel.cmisObjectType)
      Log_Internal("GetObjectByPath", path)

      Dim objectId As String = If("/".Equals(path) OrElse String.IsNullOrEmpty(path), "root", path.Substring(1).Replace("/", "\"))

      Return Object_Internal(objectId)
   End Function

   Protected Overrides Function GetAllowableActions(repositoryId As String, id As String) As Result(Of cmisAllowableActionsType)
      Log_Internal("GetAllowableActions", id)

      Dim obj As CmisObjectModel.ServiceModel.cmisObjectType = Object_Internal(id)
      Return obj.AllowableActions
   End Function

#End Region

#Region "Properties"

   Protected Overrides Function UpdateProperties(repositoryId As String, objectId As String, properties As cmisPropertiesType, changeToken As String) As Result(Of CmisObjectModel.ServiceModel.cmisObjectType)
      Log_Internal("UpdateProperties", objectId)

      UpdateProperties_Internal(objectId, properties, changeToken)

      Return Object_Internal(objectId)
   End Function

#End Region

#Region "Versionen"

   Protected Overrides Function GetAllVersions(repositoryId As String, objectId As String, versionSeriesId As String, filter As String, includeAllowableActions As Boolean?) As Result(Of CmisObjectModel.ServiceModel.cmisObjectListType)
      Log_Internal("GetAllVersions", objectId, versionSeriesId)

      Dim versions As CmisObjectModel.ServiceModel.cmisObjectListType = GetAllVersions_Internal(objectId)

      Return versions
   End Function

#End Region

#Region "CheckOut/CheckIn"

   Protected Overrides Function CheckOut(repositoryId As String, objectId As String) As Result(Of CmisObjectModel.ServiceModel.cmisObjectType)
      Log_Internal("CheckOut", objectId)

      Dim pwcId As String = CheckOut_Internal(objectId)

      Return Object_Internal(pwcId)
   End Function

   Protected Overrides Function CancelCheckOut(repositoryId As String, objectId As String) As Exception
      Log_Internal("CancelCheckOut", objectId)

      CancelCheckOut_Internal(objectId)

      Return Nothing
   End Function

   Protected Overrides Function CheckIn(repositoryId As String, objectId As String, properties As cmisPropertiesType, policies() As String, content As cmisContentStreamType, major As Boolean, checkInComment As String, Optional addACEs As cmisAccessControlListType = Nothing, Optional removeACEs As cmisAccessControlListType = Nothing) As Result(Of CmisObjectModel.ServiceModel.cmisObjectType)
      Log_Internal("CheckIn", objectId)

      Dim checkedInId As String = CheckIn_Internal(objectId, properties, policies, content, major, checkInComment, addACEs, removeACEs)

      Return Object_Internal(checkedInId)
   End Function

#End Region

#Region "Content"

   Protected Overrides Function GetContentStream(repositoryId As String, objectId As String, streamId As String) As Result(Of cmisContentStreamType)
      Log_Internal("GetContentStream", objectId)

      Dim pos As Integer = objectId.LastIndexOf(";")
      Dim versionSeriesId As String = If(pos > 0, objectId.Substring(0, pos), objectId)

      Dim meta As Metadata = DocumentMetadata_Internal(versionSeriesId)

      Dim path As String
      If objectId.EndsWith(";pwc") Then
         path = IO.Path.Combine(_folder, versionSeriesId, "pwc")
      Else
         path = IO.Path.Combine(_folder, versionSeriesId, "Versionen", meta.LabelOfLatestVersion)
      End If

      Using stream As New IO.FileStream(path, IO.FileMode.Open)
         Dim content As New cmisContentStreamType(stream, versionSeriesId.Split("\").Last, DocumentMetadata_Internal(objectId).MimeType)

         Return content
      End Using
   End Function

   Protected Overrides Function SetContentStream(repositoryId As String, objectId As String, contentStream As IO.Stream, mimeType As String, fileName As String, overwriteFlag As Boolean, changeToken As String) As Result(Of setContentStreamResponse)
      Log_Internal("SetContentStream", objectId, fileName, mimeType)

      Dim lastModificationDate As DateTime = SetContentStream_Internal(objectId, contentStream, mimeType, fileName, overwriteFlag, changeToken)

      Return New setContentStreamResponse(objectId, lastModificationDate, enumSetContentStreamResult.Created)
   End Function

#End Region

#Region "Create/Delete"

   Protected Overrides Function CreateFolder(repositoryId As String, newFolder As CmisObjectModel.Core.cmisObjectType, parentFolderId As String, addACEs As cmisAccessControlListType, removeACEs As cmisAccessControlListType) As Result(Of CmisObjectModel.ServiceModel.cmisObjectType)
      Log_Internal("CreateFolder", newFolder.Properties.GetProperties("cmis:name").First.Value.Value, parentFolderId)

      Dim name As String = newFolder.Properties.GetProperties("cmis:name").First.Value.Value
      Dim description As String = Nothing
      If (newFolder.Properties.GetProperties("cmis:description").Count > 0) Then
         description = newFolder.Properties.GetProperties("cmis:description").First.Value.Value
      End If

      Dim path As String
      Dim objectId As String
      If "root".Equals(parentFolderId) Then
         path = IO.Path.Combine(_folder, name)
         objectId = name
      Else
         path = IO.Path.Combine(_folder, parentFolderId, name)
         objectId = IO.Path.Combine(parentFolderId, name)
      End If

      If Exists_Internal(objectId) Then
         Throw New Exception("Object '" & objectId & "' exists!")
      End If

      IO.Directory.CreateDirectory(path)

      Return Object_Internal(objectId)
   End Function

   Protected Overrides Function CreateDocument(repositoryId As String, newDocument As CmisObjectModel.Core.cmisObjectType, folderId As String, content As cmisContentStreamType, versioningState As enumVersioningState?, addACEs As cmisAccessControlListType, removeACEs As cmisAccessControlListType) As Result(Of CmisObjectModel.ServiceModel.cmisObjectType)
      Log_Internal("CreateDocument", folderId, versioningState)

      '1. Eigenschaften

      Dim name As String = newDocument.Name
      Dim description As String = newDocument.Description
      Dim akte As String() = Nothing
      If newDocument.GetProperties("patorg:akte").Count > 0 Then
         akte = newDocument.GetProperties("patorg:akte").First.Value.Values
      End If

      Dim mimeType As String = Nothing
      If content IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(content.MimeType) Then
         mimeType = content.MimeType
      Else
         mimeType = Helper.MimeType(name)
      End If

      If String.IsNullOrEmpty(name) Then
         If content IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(content.Filename) AndAlso content.Filename.Contains(".") Then
            name = content.Filename
         Else
            name = Guid.NewGuid().ToString("N")
         End If
      End If

      '2. Object

      Dim path As String
      Dim originalObjectId As String
      If "root".Equals(folderId) Then
         path = IO.Path.Combine(_folder, name)
         originalObjectId = name
      Else
         path = IO.Path.Combine(_folder, folderId, name)
         originalObjectId = IO.Path.Combine(folderId, name)
      End If

      If Exists_Internal(originalObjectId) Then
         Throw New Exception("Object '" & originalObjectId & "' already exists!")
      End If

      IO.Directory.CreateDirectory(path)
      If Not versioningState = enumVersioningState.checkedout Then
         IO.Directory.CreateDirectory(IO.Path.Combine(path, "Versionen"))
      End If

      '3. Metadaten

      Dim meta As New Metadata
      meta.CreatedBy = CurrentAuthenticationInfo.User
      meta.CreationDate = DateTime.Now.ToUniversalTime()
      meta.LastModifiedBy = CurrentAuthenticationInfo.User
      meta.LastModificationDate = DateTime.Now.ToUniversalTime()
      meta.MimeType = mimeType

      meta.MajorOfLatestVersion = If(versioningState = enumVersioningState.major, 1, 0)
      meta.MinorOfLatestVersion = If(versioningState = enumVersioningState.minor, 1, 0)

      If versioningState = enumVersioningState.checkedout Then
         meta.VersionSeriesCheckedOutBy = CurrentAuthenticationInfo.User
         meta.DescriptionPwc = description
         meta.AktePwc = akte
      Else
         meta.Description = description
         meta.Akte = akte
         meta.AddComment("create")
      End If

      Dim xml As String = meta.ToXml()
      IO.File.WriteAllText(IO.Path.Combine(path, "metadata"), xml)

      '4. Content

      Dim contentPath As String
      If versioningState = enumVersioningState.checkedout Then
         contentPath = IO.Path.Combine(path, "pwc")
      Else
         contentPath = IO.Path.Combine(path, "Versionen", meta.LabelOfLatestVersion)
      End If

      If content IsNot Nothing AndAlso content.BinaryStream IsNot Nothing Then
         Using fileStream As New IO.FileStream(contentPath, IO.FileMode.Create)
            content.BinaryStream.CopyTo(fileStream)
         End Using
      Else
         IO.File.Create(contentPath).Dispose()
      End If

      '5. Rückgabewert

      Dim objectId As String = originalObjectId & If(versioningState = enumVersioningState.checkedout, ";pwc", String.Empty)

      If versioningState = enumVersioningState.checkedout Then
         'Damit die angelegte Arbeitskopie über die versionSeriesId auffindbar ist, muss es eine Version in der Versionsserie geben.
         'Deshalb hier ein CheckIn und danach ein CheckOut, damit eine Version existiert und der Status wieder 'checkedout' ist.
         '(Beachte auch CmisServiceImpl_Internal.CancleCheckOut)
         Dim checkedInId As String = CheckIn_Internal(objectId, Nothing, Nothing, Nothing, False, "enumVersioningState.checkedout")
         objectId = CheckOut_Internal(checkedInId)
      End If

      Return Object_Internal(objectId)
   End Function

   Protected Overrides Function DeleteObject(repositoryId As String, objectId As String, allVersions As Boolean) As Exception
      Log_Internal("DeleteObject", objectId, allVersions)

      If Not Exists_Internal(objectId) Then
         Return New Exception("Object '" & objectId & "' not exists!")
      End If

      Dim ex As Exception = Nothing

      If objectId.EndsWith(";pwc") Then
         CancelCheckOut_Internal(objectId)
      Else
         Dim versionSeriesId As String
         If objectId.Contains(";") Then
            Dim pos As Integer = objectId.LastIndexOf(";")
            versionSeriesId = objectId.Substring(0, pos)
         Else
            versionSeriesId = objectId
         End If

         Dim obj As CmisObjectModel.ServiceModel.cmisObjectType = Object_Internal(versionSeriesId)
         If "cmis:document".Equals(obj.ObjectTypeId) AndAlso Not allVersions Then Return NotSupported_Internal("DeleteObject(allVersions=False, cmis:objectTypeId=cmis:document")

         Try
            Dim path As String = IO.Path.Combine(_folder, versionSeriesId)

            IO.Directory.Delete(path, True)
         Catch ex
         End Try

      End If

      Return ex
   End Function

   Protected Overrides Function DeleteTree(repositoryId As String, folderId As String, allVersions As Boolean, unfileObjects As enumUnfileObject?, continueOnFailure As Boolean) As Result(Of deleteTreeResponse)
      Log_Internal("DeleteTree", folderId, allVersions, continueOnFailure)

      Dim path As String = IO.Path.Combine(_folder, folderId)

      IO.Directory.Delete(path, True)

      Return New deleteTreeResponse(enumDeleteTreeResult.OK) ' With {.FailedToDelete = New failedToDelete}
   End Function

#End Region

   '---------------------------------------------------------------------------------------------------------------------------

#Region "Not Implemented"

   Protected Overrides Function AddObjectToFolder(repositoryId As String, objectId As String, folderId As String, allVersions As Boolean) As Result(Of CmisObjectModel.ServiceModel.cmisObjectType)
      Return NotSupported_Internal("AddObjectToFolder")
   End Function

   Protected Overrides Function AppendContentStream(repositoryId As String, objectId As String, contentStream As IO.Stream, mimeType As String, fileName As String, isLastChunk As Boolean, changeToken As String) As Result(Of setContentStreamResponse)
      Return NotSupported_Internal("AppendContentStream")
   End Function

   Protected Overrides Function ApplyACL(repositoryId As String, objectId As String, addACEs As cmisAccessControlListType, removeACEs As cmisAccessControlListType, aclPropagation As enumACLPropagation) As Result(Of cmisAccessControlListType)
      Return NotSupported_Internal("ApplyACL")
   End Function

   Protected Overrides Function ApplyPolicy(repositoryId As String, objectId As String, policyId As String) As Result(Of CmisObjectModel.ServiceModel.cmisObjectType)
      Return NotSupported_Internal("ApplyPolicy")
   End Function

   Protected Overrides Function BulkUpdateProperties(repositoryId As String, data As cmisBulkUpdateType) As Result(Of CmisObjectModel.ServiceModel.cmisObjectListType)
      Return NotSupported_Internal("BulkUpdateProperties")
   End Function

   Protected Overrides Function CreateDocumentFromSource(repositoryId As String, sourceId As String, properties As cmisPropertiesType, folderId As String, versioningState As enumVersioningState?, policies() As String, addACEs As cmisAccessControlListType, removeACEs As cmisAccessControlListType) As Result(Of CmisObjectModel.ServiceModel.cmisObjectType)
      Return NotSupported_Internal("CreateDocumentFromSource")
   End Function

   Protected Overrides Function CreateItem(repositoryId As String, newItem As CmisObjectModel.Core.cmisObjectType, folderId As String, addACEs As cmisAccessControlListType, removeACEs As cmisAccessControlListType) As Result(Of CmisObjectModel.ServiceModel.cmisObjectType)
      Return NotSupported_Internal("CreateItem")
   End Function

   Protected Overrides Function CreatePolicy(repositoryId As String, newPolicy As CmisObjectModel.Core.cmisObjectType, folderId As String, addACEs As cmisAccessControlListType, removeACEs As cmisAccessControlListType) As Result(Of CmisObjectModel.ServiceModel.cmisObjectType)
      Return NotSupported_Internal("CreatePolicy")
   End Function

   Protected Overrides Function CreateRelationship(repositoryId As String, newRelationship As CmisObjectModel.Core.cmisObjectType, addACEs As cmisAccessControlListType, removeACEs As cmisAccessControlListType) As Result(Of CmisObjectModel.ServiceModel.cmisObjectType)
      Return NotSupported_Internal("CreateRelationship")
   End Function

   Protected Overrides Function CreateType(repositoryId As String, newType As cmisTypeDefinitionType) As Result(Of cmisTypeDefinitionType)
      Return NotSupported_Internal("CreateType")
   End Function

   Protected Overrides Function DeleteContentStream(repositoryId As String, objectId As String, changeToken As String) As Result(Of deleteContentStreamResponse)
      Return NotSupported_Internal("DeleteContentStream")
   End Function

   Protected Overrides Function DeleteType(repositoryId As String, typeId As String) As Exception
      NotSupported_Internal("DeleteType")
      Throw New NotImplementedException("DeleteType")
   End Function

   Protected Overrides Function GetACL(repositoryId As String, objectId As String, onlyBasicPermissions As Boolean) As Result(Of cmisAccessControlListType)
      Return NotSupported_Internal("GetACL")
   End Function

   Protected Overrides Function GetAppliedPolicies(repositoryId As String, objectId As String, filter As String) As Result(Of CmisObjectModel.ServiceModel.cmisObjectListType)
      Return NotSupported_Internal("GetAppliedPolicies")
   End Function

   Protected Overrides Function GetBaseObjectType(repositoryId As String, objectId As String) As enumBaseObjectTypeIds
      NotSupported_Internal("GetBaseObjectType")
      Throw New NotImplementedException("GetBaseObjectType")
   End Function

   Protected Overrides Function GetCheckedOutDocs(repositoryId As String, folderId As String, filter As String, maxItems As Long?, skipCount As Long?, renditionFilter As String, includeAllowableActions As Boolean?, includeRelationships As enumIncludeRelationships?) As Result(Of CmisObjectModel.ServiceModel.cmisObjectListType)
      Return NotSupported_Internal("GetCheckedOutDocs")
   End Function

   Protected Overrides Function GetContentChanges(repositoryId As String, filter As String, maxItems As Long?, includeACL As Boolean?, includePolicyIds As Boolean, includeProperties As Boolean, ByRef changeLogToken As String) As Result(Of getContentChanges)
      Return NotSupported_Internal("GetContentChanges")
   End Function

   Protected Overrides Function GetDescendants(repositoryId As String, folderId As String, filter As String, depth As Long?, includeAllowableActions As Boolean?, includeRelationships As enumIncludeRelationships?, renditionFilter As String, includePathSegment As Boolean) As Result(Of CmisObjectModel.ServiceModel.cmisObjectInFolderContainerType)
      Return NotSupported_Internal("GetDescendants")
   End Function

   Protected Overrides Function GetFolderTree(repositoryId As String, folderId As String, filter As String, depth As Long?, includeAllowableActions As Boolean?, includeRelationships As enumIncludeRelationships?, includePathSegment As Boolean, renditionFilter As String) As Result(Of CmisObjectModel.ServiceModel.cmisObjectInFolderContainerType)
      Return NotSupported_Internal("GetFolderTree")
   End Function

   Protected Overrides Function GetObjectRelationships(repositoryId As String, objectId As String, includeSubRelationshipTypes As Boolean, relationshipDirection As enumRelationshipDirection?, typeId As String, maxItems As Long?, skipCount As Long?, filter As String, includeAllowableActions As Boolean?) As Result(Of CmisObjectModel.ServiceModel.cmisObjectListType)
      Return NotSupported_Internal("GetObjectRelationships")
   End Function

   Protected Overrides Function GetUnfiledObjects(repositoryId As String, maxItems As Long?, skipCount As Long?, filter As String, includeAllowableActions As Boolean?, includeRelationships As enumIncludeRelationships?, renditionFilter As String, orderBy As String) As Result(Of CmisObjectModel.ServiceModel.cmisObjectListType)
      Return NotSupported_Internal("GetUnfiledObjects")
   End Function

   Protected Overrides Function MoveObject(repositoryId As String, objectId As String, targetFolderId As String, sourceFolderId As String) As Result(Of CmisObjectModel.ServiceModel.cmisObjectType)
      Return NotSupported_Internal("MoveObject")
   End Function

   Protected Overrides Function Query(repositoryId As String, q As String, searchAllVersions As Boolean, includeRelationships As enumIncludeRelationships?, renditionFilter As String, includeAllowableActions As Boolean, maxItems As Long?, skipCount As Long?) As Result(Of CmisObjectModel.ServiceModel.cmisObjectListType)
      Return NotSupported_Internal("Query")
   End Function

   Protected Overrides Function RemoveObjectFromFolder(repositoryId As String, objectId As String, folderId As String) As Result(Of CmisObjectModel.ServiceModel.cmisObjectType)
      Return NotSupported_Internal("RemoveObjectFromFolder")
   End Function

   Protected Overrides Function RemovePolicy(repositoryId As String, objectId As String, policyId As String) As Exception
      Return NotSupported_Internal("RemovePolicy")
   End Function

   Protected Overrides Function UpdateType(repositoryId As String, modifiedType As cmisTypeDefinitionType) As Result(Of cmisTypeDefinitionType)
      Return NotSupported_Internal("UpdateType")
   End Function

#End Region

End Class
