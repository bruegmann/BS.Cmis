'***********************************************************************************************************************
'* Project: CmisObjectModelLibrary
'* Copyright (c) 2014, Brügmann Software GmbH, Papenburg, All rights reserved
'*
'* Contact: opensource<at>patorg.de
'* 
'* CmisObjectModelLibrary is a VB.NET implementation of the Content Management Interoperability Services (CMIS) standard
'*
'* This file is part of CmisObjectModelLibrary.
'* 
'* This library is free software; you can redistribute it and/or
'* modify it under the terms of the GNU Lesser General Public
'* License as published by the Free Software Foundation; either
'* version 3.0 of the License, or (at your option) any later version.
'*
'* This library is distributed in the hope that it will be useful,
'* but WITHOUT ANY WARRANTY; without even the implied warranty of
'* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
'* Lesser General Public License for more details.
'*
'* You should have received a copy of the GNU Lesser General Public
'* License along with this library (lgpl.txt).
'* If not, see <http://www.gnu.org/licenses/lgpl.txt>.
'***********************************************************************************************************************
Imports CmisObjectModel.Client.Generic
Imports CmisObjectModel.Common
Imports CmisObjectModel.Constants
Imports cmr = CmisObjectModel.Messaging.Requests
Imports sn = System.Net
Imports ss = System.ServiceModel
Imports ssc = System.Security.Cryptography
Imports ssw = System.ServiceModel.Web
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_Integer = "Integer" OrElse xs_Integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Client
   ''' <summary>
   ''' Simplifies requests to cmis document, cmis folder, cmis policy, cmis relationship, cmis TypeDefinition, cmis repository
   ''' </summary>
   ''' <remarks></remarks>
   Public MustInherit Class CmisDataModelObject

      Protected Const VersioningStateCheckedOutPrefix As String = "{6cb02a6d-eccb-4126-a6ac-2a27cf76210e}"

#Region "Constructors"
      Protected Sub New(client As Contracts.ICmisClient, repositoryInfo As Core.cmisRepositoryInfoType)
         _client = client
         _repositoryInfo = repositoryInfo
      End Sub
#End Region

#Region "Helper classes"
      ''' <summary>
      ''' Some cmis repositories do not fulfil the cmis-specification specified in
      ''' http://docs.oasis-open.org/cmis/CMIS/v1.1/errata01/os/CMIS-v1.1-errata01-os-complete.html#x1-3280002
      ''' When creating a new document with the versioningState checkedout the document must be deleted completely if
      ''' the checkedout state of the document is terminated by a CancelCheckOut()-call. To make the client be able
      ''' to detect documents created by
      '''   CreateDocument(versioningState:=checkedout)
      ''' the description of the document (cmis:description) is marked with a predefined GUID
      ''' </summary>
      Private Class CreateDocumentService

         Public Sub New(owner As CmisDataModelObject,
                        properties As Core.Collections.cmisPropertiesType,
                        folderId As String,
                        contentStream As Messaging.cmisContentStreamType,
                        versioningState As Core.enumVersioningState?,
                        policies As Core.Collections.cmisListOfIdsType,
                        addACEs As Core.Security.cmisAccessControlListType,
                        removeACEs As Core.Security.cmisAccessControlListType)
            _owner = owner
            If properties Is Nothing AndAlso versioningState.HasValue AndAlso versioningState.Value = Core.enumVersioningState.checkedout Then
               _properties = New Core.Collections.cmisPropertiesType()
            Else
               _properties = properties
            End If
            _folderId = folderId
            _contentStream = contentStream
            _versioningState = versioningState
            _policies = policies
            _addACEs = addACEs
            _removeACEs = removeACEs
         End Sub

         Protected _addACEs As Core.Security.cmisAccessControlListType
         Private _contentStream As Messaging.cmisContentStreamType
         Protected _description As String
         Protected _folderId As String

         Public Function Invoke() As CmisDocument
            Dim checkedOutRequired As Boolean = _versioningState.HasValue AndAlso _versioningState.Value = Core.enumVersioningState.checkedout

            _owner._lastException = Nothing
            Try
               'make sure the client is able to detect a newly created document with versioning state with checkedout
               If checkedOutRequired Then LabelDescription()
               With InvokeCore()
                  _owner._lastException = .Exception
                  If _owner._lastException Is Nothing Then
                     Dim retVal As CmisDocument = TryCast(_owner.GetObject(.Response), CmisDocument)
                     'if the return document is valid,
                     If retVal IsNot Nothing AndAlso checkedOutRequired Then
                        '... but is not the requested pwc-version
                        If Not (retVal.IsPrivateWorkingCopy.HasValue AndAlso retVal.IsPrivateWorkingCopy.Value) Then
                           '... we have to request for the pwc; in case of repositories not supporting the versionstate-parameter we have to check out the
                           '    newly created document
                           If (retVal.IsVersionSeriesCheckedOut.HasValue AndAlso retVal.IsVersionSeriesCheckedOut.Value) OrElse
                              Not retVal.CheckOut() Then
                              retVal = retVal.GetObjectOfLatestVersion(acceptPWC:=enumCheckedOutState.checkedOutByMe)
                           End If
                        End If
                        'reset description property in pwc
                        retVal.Description = _description
                        Dim descriptionProperty As Core.Properties.cmisPropertyString =
                           retVal.Object.Properties.FindProperty(Of Core.Properties.cmisPropertyString)(CmisPredefinedPropertyNames.Description)
                        If descriptionProperty IsNot Nothing Then retVal.UpdateProperties(New Core.Collections.cmisPropertiesType(descriptionProperty))
                     End If

                     Return retVal
                  End If
               End With
            Catch ex As Exception
               _owner._lastException = New System.ServiceModel.Web.WebFaultException(Of Exception)(ex, sn.HttpStatusCode.BadRequest)
            End Try

            Return Nothing
         End Function

         ''' <summary>
         ''' CreateDocument()
         ''' </summary>
         Protected Overridable Function InvokeCore() As Generic.Response(Of String)
            With _owner._client.CreateDocument(New cmr.createDocument() With {.RepositoryId = _owner._repositoryInfo.RepositoryId,
                                                                              .Properties = _properties, .FolderId = _folderId,
                                                                              .ContentStream = _contentStream,
                                                                              .VersioningState = _versioningState,
                                                                              .Policies = _policies,
                                                                              .AddACEs = _addACEs, .RemoveACEs = _removeACEs})
               If .Exception IsNot Nothing Then
                  Return .Exception
               Else
                  Return .Response.ObjectId
               End If
            End With
         End Function

         ''' <summary>
         ''' Mark description property
         ''' </summary>
         Protected Overridable Sub LabelDescription()
            Dim descriptionProperty As Core.Properties.cmisProperty = Nothing
            Dim properties As Dictionary(Of String, Core.Properties.cmisProperty) =
               _properties.GetProperties(enumKeySyntax.original, CmisPredefinedPropertyNames.Description, CmisPredefinedPropertyNames.ObjectTypeId)
            Dim objectTypeId As String

            If properties.ContainsKey(CmisPredefinedPropertyNames.Description) Then
               descriptionProperty = properties(CmisPredefinedPropertyNames.Description)
            Else
               Dim cmisType As CmisType

               objectTypeId = If(properties.ContainsKey(CmisPredefinedPropertyNames.ObjectTypeId),
                                 CStr(properties(CmisPredefinedPropertyNames.ObjectTypeId).Value), Nothing)
               If String.IsNullOrEmpty(objectTypeId) Then objectTypeId = Core.Definitions.Types.cmisTypeDocumentDefinitionType.TargetTypeName
               cmisType = _owner.GetTypeDefinition(objectTypeId)
               If cmisType IsNot Nothing Then
                  Dim pdc As Dictionary(Of String, Core.Definitions.Properties.cmisPropertyDefinitionType) =
                     cmisType.Type.GetPropertyDefinitions(CmisPredefinedPropertyNames.Description)
                  If pdc.Count = 1 Then
                     descriptionProperty = pdc(CmisPredefinedPropertyNames.Description).CreateProperty()
                     _properties.Append(descriptionProperty)
                  End If
               End If
            End If
            If descriptionProperty IsNot Nothing Then
               _description = CStr(descriptionProperty.Value)
               descriptionProperty.Value = VersioningStateCheckedOutPrefix & _description
            End If
         End Sub

         Protected _owner As CmisDataModelObject
         Protected _policies As Core.Collections.cmisListOfIdsType
         Protected _properties As Core.Collections.cmisPropertiesType
         Protected _removeACEs As Core.Security.cmisAccessControlListType
         Protected _versioningState As Core.enumVersioningState?

      End Class

      ''' <summary>
      ''' Some cmis repositories do not fulfil the cmis-specification specified in
      ''' http://docs.oasis-open.org/cmis/CMIS/v1.1/errata01/os/CMIS-v1.1-errata01-os-complete.html#x1-3280002
      ''' When creating a new document with the versioningState checkedout the document must be deleted completely if
      ''' the checkedout state of the document is terminated by a CancelCheckOut()-call. To make the client be able
      ''' to detect documents created by
      '''   CreateDocumentFromSource(versioningState:=checkedout)
      ''' the description of the document (cmis:description) is marked with a predefined GUID
      ''' </summary>
      Private Class CreateDocumentFromSourceService
         Inherits CreateDocumentService

         Public Sub New(owner As CmisDataModelObject,
                        sourceId As String,
                        properties As Core.Collections.cmisPropertiesType,
                        folderId As String,
                        versioningState As Core.enumVersioningState?,
                        policies As Core.Collections.cmisListOfIdsType,
                        addACEs As Core.Security.cmisAccessControlListType,
                        removeACEs As Core.Security.cmisAccessControlListType)
            MyBase.New(owner, properties, folderId, Nothing, versioningState, policies, addACEs, removeACEs)
            _sourceId = sourceId
         End Sub

         ''' <summary>
         ''' CreateDocumentFromSource
         ''' </summary>
         Protected Overrides Function InvokeCore() As Response(Of String)
            With _owner._client.CreateDocumentFromSource(New cmr.createDocumentFromSource() With {.RepositoryId = _owner._repositoryInfo.RepositoryId,
                                                                                                  .SourceId = _sourceId, .Properties = _properties,
                                                                                                  .FolderId = _folderId,
                                                                                                  .VersioningState = _versioningState,
                                                                                                  .Policies = _policies,
                                                                                                  .AddACEs = _addACEs, .RemoveACEs = _removeACEs})
               If .Exception IsNot Nothing Then
                  Return .Exception
               Else
                  Return .Response.ObjectId
               End If
            End With
         End Function

         ''' <summary>
         ''' Mark description property
         ''' </summary>
         Protected Overrides Sub LabelDescription()
            Dim descriptionProperty As Core.Properties.cmisProperty = Nothing
            Dim properties As Dictionary(Of String, Core.Properties.cmisProperty) =
               _properties.GetProperties(enumKeySyntax.original, CmisPredefinedPropertyNames.Description, CmisPredefinedPropertyNames.ObjectTypeId)

            If properties.ContainsKey(CmisPredefinedPropertyNames.Description) Then
               descriptionProperty = properties(CmisPredefinedPropertyNames.Description)
            Else
               Dim source As CmisObject = _owner.GetObject(_sourceId, CmisPredefinedPropertyNames.Description)
               If source IsNot Nothing AndAlso source.Properties IsNot Nothing Then
                  descriptionProperty = source.Properties.FindProperty(CmisPredefinedPropertyNames.Description)
                  If descriptionProperty IsNot Nothing Then _properties.Append(descriptionProperty)
               End If
            End If
            If descriptionProperty IsNot Nothing Then
               _description = CStr(descriptionProperty.Value)
               descriptionProperty.Value = VersioningStateCheckedOutPrefix & _description
            End If
         End Sub

         Private _sourceId As String
      End Class
#End Region

#Region "Repository"
      ''' <summary>
      ''' Returns the list of object-types defined for the repository that are children of the specified type
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Function GetTypeChildren(typeId As String,
                                         includePropertyDefinitions As Boolean,
                                         maxItems As xs_Integer?,
                                         skipCount As xs_Integer?) As Generic.ItemList(Of CmisType)
         With _client.GetTypeChildren(New cmr.getTypeChildren() With {.RepositoryId = _repositoryInfo.RepositoryId, .TypeId = typeId,
                                                                      .IncludePropertyDefinitions = includePropertyDefinitions,
                                                                      .MaxItems = maxItems, .SkipCount = skipCount})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Dim types = .Response.Types
               Dim hasMoreItems As Boolean = False
               Dim numItems As xs_Integer? = Nothing
               Dim result As CmisType() = New CmisType() {}

               If types IsNot Nothing Then
                  hasMoreItems = types.HasMoreItems
                  numItems = types.NumItems
                  If types.Types IsNot Nothing Then
                     result = (From type As Core.Definitions.Types.cmisTypeDefinitionType In types.Types
                               Select CreateCmisType(type)).ToArray()
                  End If
               End If

               Return New Generic.ItemList(Of CmisType)(result, hasMoreItems, numItems)
            Else
               Return Nothing
            End If
         End With
      End Function

      ''' <summary>
      ''' Gets the definition of the specified object-type
      ''' </summary>
      ''' <remarks></remarks>
      Protected Overridable Function GetTypeDefinition(typeId As String) As CmisType
         With _client.GetTypeDefinition(New cmr.getTypeDefinition() With {.RepositoryId = _repositoryInfo.RepositoryId, .TypeId = typeId})
            _lastException = .Exception
            Return If(_lastException Is Nothing, CreateCmisType(.Response.Type), Nothing)
         End With
      End Function

      ''' <summary>
      ''' Returns the set of the descendant object-types defined for the Repository under the specified type
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Function GetTypeDescendants(typeId As String, depth As xs_Integer?,
                                            includePropertyDefinitions As Boolean) As Generic.ItemContainer(Of CmisType)()
         With _client.GetTypeDescendants(New cmr.getTypeDescendants() With {.RepositoryId = _repositoryInfo.RepositoryId,
                                                                            .TypeId = typeId, .Depth = depth,
                                                                            .IncludePropertyDefinitions = includePropertyDefinitions})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Return Transform(.Response.Types, New List(Of Generic.ItemContainer(Of CmisType))).ToArray()
            Else
               Return Nothing
            End If
         End With
      End Function
#End Region

#Region "Navigation"
      ''' <summary>
      ''' Gets the list of documents that are checked out that the user has access to
      ''' </summary>
      ''' <remarks></remarks>
      Protected Function GetCheckedOutDocs(folderId As String,
                                           maxItems As xs_Integer?,
                                           skipCount As xs_Integer?,
                                           orderBy As String,
                                           filter As String,
                                           includeRelationships As Core.enumIncludeRelationships?,
                                           renditionFilter As String,
                                           includeAllowableActions As Boolean?) As Generic.ItemList(Of CmisObject)
         With _client.GetCheckedOutDocs(New cmr.getCheckedOutDocs() With {.RepositoryId = _repositoryInfo.RepositoryId, .FolderId = folderId,
                                                                          .MaxItems = maxItems, .SkipCount = skipCount, .OrderBy = orderBy,
                                                                          .Filter = filter, .IncludeRelationships = includeRelationships,
                                                                          .RenditionFilter = renditionFilter,
                                                                          .IncludeAllowableActions = includeAllowableActions})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Return Convert(.Response.Objects)
            Else
               Return Nothing
            End If
         End With
      End Function
#End Region

#Region "Object"
      ''' <summary>
      ''' Creates a document object of the specified type (given by the cmis:objectTypeId property) in the (optionally) specified location
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Function CreateDocument(properties As Core.Collections.cmisPropertiesType,
                                        Optional folderId As String = Nothing,
                                        Optional contentStream As Messaging.cmisContentStreamType = Nothing,
                                        Optional versioningState As Core.enumVersioningState? = Nothing,
                                        Optional policies As Core.Collections.cmisListOfIdsType = Nothing,
                                        Optional addACEs As Core.Security.cmisAccessControlListType = Nothing,
                                        Optional removeACEs As Core.Security.cmisAccessControlListType = Nothing) As CmisDocument
         With New CreateDocumentService(Me, properties, folderId, contentStream, versioningState, policies, addACEs, removeACEs)
            Return .Invoke()
         End With
      End Function

      ''' <summary>
      ''' Creates a document object as a copy of the given source document in the (optionally) specified location
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Function CreateDocumentFromSource(sourceId As String,
                                                  Optional properties As Core.Collections.cmisPropertiesType = Nothing,
                                                  Optional folderId As String = Nothing,
                                                  Optional versioningState As Core.enumVersioningState? = Nothing,
                                                  Optional policies As Core.Collections.cmisListOfIdsType = Nothing,
                                                  Optional addACEs As Core.Security.cmisAccessControlListType = Nothing,
                                                  Optional removeACEs As Core.Security.cmisAccessControlListType = Nothing) As CmisDocument
         With New CreateDocumentFromSourceService(Me, sourceId, properties, folderId, versioningState, policies, addACEs, removeACEs)
            Return .Invoke()
         End With
      End Function

      ''' <summary>
      ''' Gets the specified information for the object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Function GetObject(objectId As String,
                                   Optional filter As String = Nothing, Optional includeRelationships As Core.enumIncludeRelationships? = Nothing,
                                   Optional includePolicyIds As Boolean? = Nothing, Optional renditionFilter As String = Nothing,
                                   Optional includeACL As Boolean? = Nothing, Optional includeAllowableActions As Boolean? = Nothing) As CmisObject
         With _client.GetObject(New cmr.getObject() With {.RepositoryId = _repositoryInfo.RepositoryId, .ObjectId = objectId,
                                                          .Filter = filter, .IncludeRelationships = includeRelationships,
                                                          .IncludePolicyIds = includePolicyIds, .RenditionFilter = renditionFilter,
                                                          .IncludeACL = includeACL, .IncludeAllowableActions = includeAllowableActions})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Return CreateCmisObject(.Response.Object)
            Else
               Return Nothing
            End If
         End With
      End Function

      ''' <summary>
      ''' Gets the specified information for the latest object in the version series
      ''' </summary>
      ''' <param name="objectId"></param>
      ''' <param name="filter"></param>
      ''' <param name="includeRelationships"></param>
      ''' <param name="includePolicyIds"></param>
      ''' <param name="renditionFilter"></param>
      ''' <param name="includeACL"></param>
      ''' <param name="includeAllowableActions"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Function GetObjectOfLatestVersion(objectId As String, Optional major As Boolean = False,
                                                  Optional filter As String = Nothing, Optional includeRelationships As Core.enumIncludeRelationships? = Nothing,
                                                  Optional includePolicyIds As Boolean? = Nothing, Optional renditionFilter As String = Nothing,
                                                  Optional includeACL As Boolean? = Nothing, Optional includeAllowableActions As Boolean? = Nothing,
                                                  Optional acceptPWC As Common.enumCheckedOutState = enumCheckedOutState.notCheckedOut) As CmisDocument
         'returns the private working copy if exists and is accepted ...
         If acceptPWC <> enumCheckedOutState.notCheckedOut Then
            Dim retVal As CmisDocument = GetPrivateWorkingCopy(objectId, filter, includeRelationships, includePolicyIds, renditionFilter, includeACL, includeAllowableActions, acceptPWC)
            If retVal IsNot Nothing Then Return retVal
         End If
         '... otherwise returns ObjectOfLatestVersion
         With _client.GetObjectOfLatestVersion(New cmr.getObjectOfLatestVersion() With {.RepositoryId = _repositoryInfo.RepositoryId, .ObjectId = objectId,
                                                                                        .Major = major, .Filter = filter,
                                                                                        .IncludeRelationships = includeRelationships,
                                                                                        .IncludePolicyIds = includePolicyIds, .RenditionFilter = renditionFilter,
                                                                                        .IncludeACL = includeACL, .IncludeAllowableActions = includeAllowableActions})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Return TryCast(CreateCmisObject(.Response.Object), CmisDocument)
            Else
               Return Nothing
            End If
         End With
      End Function

      ''' <summary>
      ''' Get the private working copy if the current document is checked out by current user (acceptPWC=enumCheckedOutState.checkedOutByMe)
      ''' or by any user (acceptPWC=enumCheckedOutState.checkedOut)
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Function GetPrivateWorkingCopy(objectId As String,
                                               Optional filter As String = Nothing,
                                               Optional includeRelationships As Core.enumIncludeRelationships? = Nothing,
                                               Optional includePolicyIds As Boolean? = Nothing,
                                               Optional renditionFilter As String = Nothing,
                                               Optional includeACL As Boolean? = Nothing,
                                               Optional includeAllowableActions As Boolean? = Nothing,
                                               Optional acceptPWC As Common.enumCheckedOutState = enumCheckedOutState.checkedOutByMe) As CmisDocument
         If acceptPWC = enumCheckedOutState.notCheckedOut Then
            Return Nothing
         Else
            Dim propertyNames As String() = New String() {CmisPredefinedPropertyNames.IsVersionSeriesCheckedOut,
                                                          CmisPredefinedPropertyNames.ObjectId,
                                                          CmisPredefinedPropertyNames.VersionSeriesCheckedOutBy,
                                                          CmisPredefinedPropertyNames.VersionSeriesCheckedOutId,
                                                          CmisPredefinedPropertyNames.VersionSeriesId}
            Dim response = _client.GetObject(New cmr.getObject() With {.RepositoryId = _repositoryInfo.RepositoryId, .ObjectId = objectId,
                                                                       .Filter = String.Join(","c, propertyNames)})
            Dim cmisObject = If(response.Exception Is Nothing, response.Response.Object, Nothing)

            If cmisObject IsNot Nothing AndAlso cmisObject.IsVersionSeriesCheckedOut.HasValue AndAlso cmisObject.IsVersionSeriesCheckedOut.Value AndAlso
               ((GetCheckedOut(cmisObject, _client) And acceptPWC) = acceptPWC) AndAlso
               Not String.IsNullOrEmpty(cmisObject.VersionSeriesCheckedOutId) Then
               'the document is checked out
               Return TryCast(GetObject(response.Response.Object.VersionSeriesCheckedOutId, filter,
                                        includeRelationships, includePolicyIds, renditionFilter,
                                        includeACL, includeAllowableActions), CmisDocument)
            Else
               Return Nothing
            End If
         End If
      End Function

      ''' <summary>
      ''' Moves the specified file-able object from one folder to another
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Function MoveObject(objectId As String, targetFolderId As String, sourceFolderId As String) As CmisObject
         With _client.MoveObject(New cmr.moveObject() With {.RepositoryId = _repositoryInfo.RepositoryId, .ObjectId = objectId,
                                                            .TargetFolderId = targetFolderId, .SourceFolderId = sourceFolderId})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Return GetObject(.Response.ObjectId)
            Else
               Return Nothing
            End If
         End With
      End Function
#End Region

#Region "Browser Binding support"
      Public Overridable Sub BeginSuccinct(succinct As Boolean)
         If _client.SupportsSuccinct Then Browser.SuccinctSupport.BeginSuccinct(succinct)
      End Sub
      Public Overridable Sub BeginToken(token As Browser.TokenGenerator)
         If _client.SupportsToken Then Browser.TokenGenerator.BeginToken(token)
      End Sub
      Public Overridable ReadOnly Property CurrentSuccinct As Boolean
         Get
            Return _client.SupportsSuccinct AndAlso Browser.SuccinctSupport.Current
         End Get
      End Property
      Public Overridable ReadOnly Property CurrentToken As Browser.TokenGenerator
         Get
            Return If(_client.SupportsToken, Browser.TokenGenerator.Current, Nothing)
         End Get
      End Property
      Public Overridable Function EndSuccinct() As Boolean
         Return _client.SupportsSuccinct AndAlso Browser.SuccinctSupport.EndSuccinct
      End Function
      Public Overridable Function EndToken() As Browser.TokenGenerator
         Return If(_client.SupportsToken, Browser.TokenGenerator.EndToken(), Nothing)
      End Function
#End Region

      Protected _client As Contracts.ICmisClient
      Public ReadOnly Property Client As Contracts.ICmisClient
         Get
            Return _client
         End Get
      End Property

      Public Overridable ReadOnly Property CmisService As CmisService
         Get
            Return New CmisService(_client)
         End Get
      End Property

      ''' <summary>
      ''' Converts cmisObjectListType into ItemList(Of CmisObject)
      ''' </summary>
      ''' <param name="objects"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Function Convert(objects As Messaging.cmisObjectListType) As Generic.ItemList(Of CmisObject)
         Dim result As New List(Of CmisObject)
         Dim hasMoreItems As Boolean = False
         Dim numItems As xs_Integer? = Nothing

         If objects IsNot Nothing Then
            hasMoreItems = objects.HasMoreItems
            numItems = objects.NumItems
            If objects.Objects IsNot Nothing Then
               For Each [object] As Core.cmisObjectType In objects.Objects
                  result.Add(CreateCmisObject([object]))
               Next
            End If
         End If

         Return New Generic.ItemList(Of CmisObject)(result.ToArray(), hasMoreItems, numItems)
      End Function

      ''' <summary>
      ''' Converts cmisObjectInFolderListType into ItemList(Of CmisObject)
      ''' </summary>
      ''' <param name="objects"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Function Convert(objects As Messaging.cmisObjectInFolderListType) As Generic.ItemList(Of CmisObject)
         Dim hasMoreItems As Boolean = False
         Dim numItems As xs_Integer? = Nothing
         Dim result As New List(Of CmisObject)

         If objects IsNot Nothing Then
            hasMoreItems = objects.HasMoreItems
            numItems = objects.NumItems
            If objects.Objects IsNot Nothing Then
               For Each [object] As Messaging.cmisObjectInFolderType In objects.Objects
                  Dim cmisObject As CmisObject = CreateCmisObject([object].Object)
                  cmisObject.PathSegment = [object].PathSegment
                  result.Add(cmisObject)
               Next
            End If
         End If

         Return New Generic.ItemList(Of CmisObject)(result.ToArray(), hasMoreItems, numItems)
      End Function

      Protected Overridable Function CreateCmisObject(cmisObject As Core.cmisObjectType) As CmisObject
         _client.Vendor.PatchProperties(_repositoryInfo, cmisObject)
         Return cmisObject + _client + _repositoryInfo
      End Function

      Protected Overridable Function CreateCmisType(type As Core.Definitions.Types.cmisTypeDefinitionType) As CmisType
         Return type + _client + _repositoryInfo
      End Function

      Protected Shared Function GetCheckedOut(cmisObject As Core.cmisObjectType,
                                              client As Contracts.ICmisClient) As Common.enumCheckedOutState
         If cmisObject Is Nothing Then
            Return enumCheckedOutState.notCheckedOut
         Else
            Dim versionSeriesCheckedOutBy As String = cmisObject.VersionSeriesCheckedOutBy

            If String.IsNullOrEmpty(versionSeriesCheckedOutBy) Then
               Return If(cmisObject.IsVersionSeriesCheckedOut, enumCheckedOutState.checkedOut, enumCheckedOutState.notCheckedOut)
            ElseIf String.Compare(cmisObject.VersionSeriesCheckedOutBy, client.User, True) = 0 Then
               Return enumCheckedOutState.checkedOutByMe
            Else
               Return enumCheckedOutState.checkedOut
            End If
         End If
      End Function

      Private _holdCapability As Boolean?
      ''' <summary>
      ''' Returns true if the repository supports holds
      ''' </summary>
      Protected ReadOnly Property HoldCapability As Boolean
         Get
            If Not _holdCapability.HasValue Then
               Dim td As CmisType = GetTypeDefinition(Core.Definitions.Types.cmisTypeRM_HoldDefinitionType.TargetTypeName)
               _holdCapability = (td IsNot Nothing)
            End If
            Return _holdCapability.Value
         End Get
      End Property

      Protected _lastException As ss.FaultException
      Public ReadOnly Property LastException As ss.FaultException
         Get
            Return _lastException
         End Get
      End Property

      Protected _repositoryInfo As Core.cmisRepositoryInfoType
      Public ReadOnly Property RepositoryInfo As Core.cmisRepositoryInfoType
         Get
            Return _repositoryInfo
         End Get
      End Property

      Private _retentionCapability As enumRetentionCapability?
      ''' <summary>
      ''' Returns the retentions supported by the repository
      ''' </summary>
      Protected ReadOnly Property RetentionCapability As enumRetentionCapability
         Get
            If Not _retentionCapability.HasValue Then
               Dim retVal As enumRetentionCapability = enumRetentionCapability.none

               If GetTypeDefinition(Core.Definitions.Types.cmisTypeRM_ClientMgtRetentionDefinitionType.TargetTypeName) IsNot Nothing Then
                  retVal = enumRetentionCapability.clientMgt
               End If
               If GetTypeDefinition(Core.Definitions.Types.cmisTypeRM_RepMgtRetentionDefinitionType.TargetTypeName) IsNot Nothing Then
                  retVal = retVal Or enumRetentionCapability.repositoryMgt
               End If
               _retentionCapability = retVal

               Return retVal
            Else
               Return _retentionCapability.Value
            End If
         End Get
      End Property

      ''' <summary>
      ''' Transforms the cmisTypeContainer()-structure into a List(Of ItemContainer(Of CmisType))-structure
      ''' </summary>
      ''' <param name="source"></param>
      ''' <param name="result"></param>
      ''' <remarks></remarks>
      Private Function Transform(source As Messaging.cmisTypeContainer(),
                                 result As List(Of Generic.ItemContainer(Of CmisType))) As List(Of Generic.ItemContainer(Of CmisType))
         result.Clear()
         If source IsNot Nothing Then
            For Each typeContainer As Messaging.cmisTypeContainer In source
               If typeContainer IsNot Nothing Then
                  Dim container As New Generic.ItemContainer(Of CmisType)(CreateCmisType(typeContainer.Type))

                  result.Add(container)
                  Transform(typeContainer.Children, container.Children)
               End If
            Next
         End If

         Return result
      End Function

   End Class
End Namespace