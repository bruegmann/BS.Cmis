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
Imports CmisObjectModel.Common
Imports CmisObjectModel.Constants
Imports ca = CmisObjectModel.AtomPub
Imports ccdp = CmisObjectModel.Core.Definitions.Properties
Imports ccdt = CmisObjectModel.Core.Definitions.Types
Imports ccg = CmisObjectModel.Common.Generic
Imports cm = CmisObjectModel.Messaging
Imports cmr = CmisObjectModel.Messaging.Responses
Imports sn = System.Net
Imports sri = System.Runtime.InteropServices
Imports ssw = System.ServiceModel.Web
Imports sss = System.ServiceModel.Syndication
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_Integer = "Integer" OrElse xs_Integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#If Not xs_HttpRequestAddRange64 Then
#Const HttpRequestAddRangeShortened = True
#End If
#End If

Namespace CmisObjectModel.Client.Browser
   ''' <summary>
   ''' Implements the functionality of a cmis-client version 1.1
   ''' </summary>
   ''' <remarks></remarks>
   Public Class CmisClient
      Inherits Base.Generic.CmisClient(Of Core.cmisRepositoryInfoType)

#Region "Constructors"
      Public Sub New(serviceDocUri As Uri, vendor As enumVendor,
                     authentication As AuthenticationProvider,
                     Optional connectTimeout As Integer? = Nothing,
                     Optional readWriteTimeout As Integer? = Nothing)
         MyBase.New(serviceDocUri, vendor, authentication, connectTimeout, readWriteTimeout)
      End Sub
#End Region

#Region "Helper classes"
      ''' <summary>
      ''' Extends the Vendors.Vendor.State class with succinct properties support
      ''' </summary>
      ''' <remarks></remarks>
      Private Class State
         Inherits Vendors.Vendor.State

         Public Sub New(repositoryId As String, succinct As Boolean)
            MyBase.New(repositoryId)
            _succinct = succinct
         End Sub

         ''' <summary>
         ''' Returns a collection of property definitions that are defined in tds
         ''' </summary>
         Private Function GetPropertyDefinitions(tds As ccdt.cmisTypeDefinitionType()) As Dictionary(Of String, ccdp.cmisPropertyDefinitionType)
            Dim retVal As New Dictionary(Of String, ccdp.cmisPropertyDefinitionType)

            For Each td As ccdt.cmisTypeDefinitionType In tds
               If td.PropertyDefinitions IsNot Nothing Then
                  For Each pd As ccdp.cmisPropertyDefinitionType In td.PropertyDefinitions
                     If pd IsNot Nothing Then
                        Dim id As String = If(pd.Id, String.Empty)
                        If Not retVal.ContainsKey(id) Then retVal.Add(id, pd)
                     End If
                  Next
               End If
            Next

            Return retVal
         End Function

         ''' <summary>
         ''' Returns all typedefinitions defined by cmis:objectTypeId and cmis:secondaryObjectTypeIds
         ''' </summary>
         ''' <param name="properties"></param>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Private Function GetTypeDefinitions(properties As Dictionary(Of String, Core.Properties.cmisProperty)) As Core.Definitions.Types.cmisTypeDefinitionType()
            Dim typeDefinitions As New List(Of Core.Definitions.Types.cmisTypeDefinitionType)

            For Each propertyName As String In New String() {CmisPredefinedPropertyNames.ObjectTypeId, CmisPredefinedPropertyNames.SecondaryObjectTypeIds}
               If properties.ContainsKey(propertyName) Then
                  Dim stringProperty As Core.Properties.Generic.cmisProperty(Of String) = TryCast(properties(propertyName), Core.Properties.Generic.cmisProperty(Of String))
                  Dim typeIds As String() = If(stringProperty Is Nothing, Nothing, stringProperty.Values)

                  If typeIds IsNot Nothing Then
                     For Each typeId As String In typeIds
                        Dim td = TypeDefinition(RepositoryId, typeId)
                        If td IsNot Nothing Then typeDefinitions.Add(td)
                     Next
                  End If
               End If
            Next

            Return typeDefinitions.ToArray()
         End Function

         Private _succinct As Boolean

         ''' <summary>
         ''' Converts DateTime properties and object properties when succinct parameter is in use
         ''' </summary>
         Public Function TransformResponse(result As Core.Collections.cmisPropertiesType()) As Core.Collections.cmisPropertiesType()
            'convert datetime properties
            If _succinct AndAlso result IsNot Nothing AndAlso Not String.IsNullOrEmpty(RepositoryId) Then
               For Each propertyCollection As Core.Collections.cmisPropertiesType In result
                  Dim propertyMap = If(propertyCollection Is Nothing, Nothing, propertyCollection.GetProperties())
                  Dim length As Integer = If(propertyMap Is Nothing, 0, propertyMap.Count)

                  If length > 0 Then
                     Dim tds = GetTypeDefinitions(propertyMap)
                     Dim pds = GetPropertyDefinitions(tds)
                     Dim containsChanges As Boolean = False
                     Dim properties As Core.Properties.cmisProperty() =
                        CType(Array.CreateInstance(GetType(Core.Properties.cmisProperty), length), Core.Properties.cmisProperty())

                     Array.Copy(propertyCollection.Properties, properties, length)
                     For index As Integer = 0 To length - 1
                        Dim cmisProperty = properties(index)

                        If cmisProperty IsNot Nothing AndAlso pds.ContainsKey(cmisProperty.PropertyDefinitionId) Then
                           Dim pd = pds(cmisProperty.PropertyDefinitionId)

                           Try
                              If pd.PropertyType = Core.enumPropertyType.datetime Then
                                 properties(index) = If(cmisProperty.Values Is Nothing, pd.CreateProperty(),
                                                        pd.CreateProperty((From value As Object In cmisProperty.Values
                                                                           Select CObj(CType(Common.FromJSONTime(CLng(value)), DateTimeOffset))).ToArray()))
                              ElseIf cmisProperty.Values Is Nothing Then
                                 properties(index) = pd.CreateProperty()
                              Else
                                 properties(index) = pd.CreateProperty(cmisProperty.Values)
                              End If
                              containsChanges = True
                           Catch
                           End Try
                        End If
                     Next

                     'at least one property has been converted
                     If containsChanges Then propertyCollection.Properties = properties
                  End If
               Next
            End If

            Return result
         End Function
      End Class

      ''' <summary>
      ''' Specific UriBuilder for browser binding
      ''' </summary>
      ''' <remarks></remarks>
      Private Class UriBuilder

#Region "Constructors"
         Public Sub New(baseUri As String, request As Messaging.Requests.RequestBase,
                        ParamArray searchAndReplace As Object())
            Me.New(baseUri, request.BrowserBinding, searchAndReplace)
         End Sub
         Public Sub New(baseUri As Uri, request As Messaging.Requests.RequestBase,
                        ParamArray searchAndReplace As Object())
            Me.New(baseUri.OriginalString, request.BrowserBinding, searchAndReplace)
         End Sub
         Private Sub New(baseUri As String, browserBindingExtensions As Messaging.Requests.RequestBase.BrowserBindingExtensions,
                         searchAndReplace As Object())
            _baseUri = baseUri

            'check queryString-parameters defined in browserBindingExtensions
            If browserBindingExtensions IsNot Nothing Then
               With browserBindingExtensions
                  If Not String.IsNullOrEmpty(.CmisSelector) AndAlso Not _replacements.ContainsKey("cmisselector") Then _replacements.Add("cmisselector", .CmisSelector)
                  If .Succinct AndAlso Not _replacements.ContainsKey("succinct") Then _replacements.Add("succinct", "true")
                  If Not String.IsNullOrEmpty(.Token) AndAlso Not _replacements.ContainsKey("token") Then _replacements.Add("token", .Token)
               End With
            End If
            'process searchAndReplace values
            If searchAndReplace IsNot Nothing Then AddRange(searchAndReplace)
         End Sub
#End Region

#Region "Add- and AddRange-methods"
         Public Sub Add(parameterName As String, value As Boolean)
            Add(parameterName, If(value, "true", "false"))
         End Sub
         Public Sub Add(parameterName As String, value As Boolean?)
            If value.HasValue Then Add(parameterName, value.Value)
         End Sub
         Public Sub Add(Of TValue As Structure)(parameterName As String, value As TValue)
            Add(parameterName, CType(CObj(value), System.Enum).GetName())
         End Sub
         Public Sub Add(Of TValue As Structure)(parameterName As String, value As TValue?)
            If value.HasValue Then Add(parameterName, value.Value)
         End Sub
         Public Sub Add(parameterName As String, value As Integer)
            Add(parameterName, CStr(value))
         End Sub
         Public Sub Add(parameterName As String, value As Integer?)
            If value.HasValue Then Add(parameterName, value.Value)
         End Sub
         Public Sub Add(parameterName As String, value As Long)
            Add(parameterName, CStr(value))
         End Sub
         Public Sub Add(parameterName As String, value As Long?)
            If value.HasValue Then Add(parameterName, value.Value)
         End Sub
         Public Sub Add(parameterName As String, value As String, Optional required As Boolean = False)
            If (required OrElse Not String.IsNullOrEmpty(value)) AndAlso Not _replacements.ContainsKey(parameterName) Then
               _replacements.Add(parameterName, value)
            End If
         End Sub

         Public Sub AddRange(ParamArray searchAndReplace As Object())
            Dim length As Integer = If(searchAndReplace Is Nothing, 0, searchAndReplace.Length)

            If length > 0 Then
               Dim queue As New Queue(Of Object)(searchAndReplace)

               While queue.Count > 1
                  Dim placeHolderName As String
                  Dim replacement As Object
                  Dim replacementType As Type
                  Dim required As Boolean
                  Dim current As Object = queue.Dequeue

                  'current must be the placeHolderName, expected type is string
                  If TypeOf current Is String Then placeHolderName = CType(current, String) Else Continue While
                  replacement = queue.Dequeue
                  replacementType = If(replacement Is Nothing, GetType(String), replacement.GetType())
                  required = If(queue.Count > 0 AndAlso TypeOf replacement Is String AndAlso TypeOf queue.Peek Is Boolean,
                                CType(queue.Dequeue, Boolean), False)
                  If replacementType Is GetType(String) Then
                     Add(placeHolderName, CType(replacement, String), required)
                  ElseIf replacementType.IsNullableType Then
                     'get a helper for genericArgumentType (for example: if replacementType is GetType(Boolean?) the expression
                     'replacementType.GetGenericArguments()(0) gets GetType(Boolean))
                     Dim helper As Common.GenericRuntimeHelper = Common.GenericRuntimeHelper.GetInstance(replacementType.GetGenericArguments()(0))
                     If helper.HasValue(replacement) Then Add(placeHolderName, helper.Convert(helper.GetValue(replacement)))
                  Else
                     Dim helper As Common.GenericRuntimeHelper = Common.GenericRuntimeHelper.GetInstance(replacementType)
                     Add(placeHolderName, helper.Convert(replacement))
                  End If
               End While
            End If
         End Sub
#End Region

         Private _baseUri As String
         Private _replacements As New Dictionary(Of String, String)

         Public Function ToUri() As Uri
            Dim indexOf As Integer = _baseUri.IndexOf("?") + 1
            Dim baseUri As String
            Dim separator As String

            If indexOf > 0 Then
               'append predefined queryString-parameters
               With System.Web.HttpUtility.ParseQueryString(_baseUri.Substring(indexOf))
                  baseUri = _baseUri.Substring(0, indexOf) & String.Join("&", (From key As String In .AllKeys
                                                                               Where Not _replacements.ContainsKey(key)
                                                                               Let value As String = .Item(key)
                                                                               Select key & "=" & System.Uri.EscapeDataString(value)))
                  separator = If(baseUri.EndsWith("?"), Nothing, "&")
               End With
            Else
               baseUri = _baseUri
               separator = "?"
            End If

            'append parameters
            If _replacements.Count > 0 Then
               baseUri = baseUri & separator & String.Join("&", (From de As KeyValuePair(Of String, String) In _replacements
                                                                 Select de.Key & "={" & de.Key & "}"))
               baseUri = baseUri.ReplaceUri(_replacements)
            End If

            Return New Uri(baseUri)
         End Function

         Public Shared Widening Operator CType(value As UriBuilder) As Uri
            Return If(value Is Nothing, Nothing, value.ToUri())
         End Operator

      End Class
#End Region

#Region "Repository"
      ''' <summary>
      ''' Creates a new type definition that is a subtype of an existing specified parent type
      ''' </summary>
      Public Overrides Function CreateType(request As Messaging.Requests.createType) As Generic.Response(Of Messaging.Responses.createTypeResponse)
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim content As New JSON.MultipartFormDataContent(MediaTypes.MultipartFormData, "createType")

               content.Add(request.Type)
               Dim result = Me.Post(New UriBuilder(.Response.RepositoryUrl, request), content, Nothing,
                                    AddressOf Deserialize(Of Core.Definitions.Types.cmisTypeDefinitionType),
                                    False)
               If result.Exception Is Nothing Then
                  Dim type As Core.Definitions.Types.cmisTypeDefinitionType = result.Response

                  'cache typeDefinition
                  If type IsNot Nothing Then
                     Dim typeId As String = type.Id
                     Dim repositoryId As String = request.RepositoryId

                     If Not (String.IsNullOrEmpty(repositoryId) OrElse String.IsNullOrEmpty(typeId)) Then
                        TypeDefinition(repositoryId, typeId) = type
                     End If
                  End If

                  Return New Messaging.Responses.createTypeResponse() With {.Type = type}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Deletes a type definition
      ''' </summary>
      Public Overrides Function DeleteType(request As Messaging.Requests.deleteType) As Generic.Response(Of Messaging.Responses.deleteTypeResponse)
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim content As New JSON.MultipartFormDataContent(MediaTypes.UrlEncodedUTF8, "deletetype")

               content.Add("typeId", request.TypeId)
               Dim result = Me.Post(New UriBuilder(.Response.RepositoryUrl, request), content, Nothing)
               If result.Exception Is Nothing Then
                  Return New Messaging.Responses.deleteTypeResponse()
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Returns all repositories
      ''' </summary>
      Public Overrides Function GetRepositories(request As Messaging.Requests.getRepositories) As Generic.Response(Of Messaging.Responses.getRepositoriesResponse)
         Dim result = Me.Get(New UriBuilder(_serviceDocUri, request), AddressOf DeserializeRepositories, False)

         If result.Exception Is Nothing Then
            Dim entries As New List(Of Messaging.cmisRepositoryEntryType)

            If result.Response IsNot Nothing Then
               For Each repository As Core.cmisRepositoryInfoType In result.Response
                  entries.Add(New Messaging.cmisRepositoryEntryType() With {.Repository = repository})
               Next
            End If
            Return New Messaging.Responses.getRepositoriesResponse() With {.Repositories = entries.ToArray()}
         Else
            Return result.Exception
         End If
      End Function

      ''' <summary>
      ''' Returns the workspace of specified repository
      ''' </summary>
      Public Overrides Function GetRepositoryInfo(request As Messaging.Requests.getRepositoryInfo, Optional ignoreCache As Boolean = False) As Generic.Response(Of Messaging.Responses.getRepositoryInfoResponse)
         Dim repositoryId As String = request.RepositoryId
         'try to get the info using the cache
         Dim repositoryInfo As Core.cmisRepositoryInfoType = If(ignoreCache, Nothing, Me.RepositoryInfo(repositoryId))

         request.BrowserBinding.CmisSelector = "repositoryInfo"
         'workspace of specified repository could not be found in the cache
         If repositoryInfo Is Nothing Then
            'perhaps the server supports an optional repositoryId-parameter, otherwise the request will get all repositories
            Dim result = Me.Get(New UriBuilder(_serviceDocUri, request, "repositoryId", repositoryId),
                                AddressOf DeserializeRepositories,
                                False)

            For index As Integer = 0 To 1
               If result.Exception IsNot Nothing Then
                  Return result.Exception
               ElseIf result.Response IsNot Nothing Then
                  'search for the requested repositoryId
                  repositoryInfo = FindRepository(result.Response, repositoryId)
                  If repositoryInfo Is Nothing Then
                     Exit For
                  Else
                     If index = 0 Then
                        'repeat the request with the RepositoryUrl of the repository to get all informations about the repository
                        '(minimum of retreived informations at this time: RepositoryId, RepositoryName, RepositoryUrl and RootFolderUrl)
                        result = Me.Get(New UriBuilder(repositoryInfo.RepositoryUrl, request),
                                        AddressOf DeserializeRepositories,
                                        False)
                        Continue For
                     Else
                        Me.RepositoryInfo(repositoryId) = repositoryInfo
                     End If
                  End If
               Else
                  Exit For
               End If
            Next
         End If

         If repositoryInfo Is Nothing Then
            Dim cmisFault As New Messaging.cmisFaultType(Net.HttpStatusCode.NotFound, Messaging.enumServiceException.objectNotFound, "Workspace not found.")
            Return cmisFault.ToFaultException()
         Else
            Return New Messaging.Responses.getRepositoryInfoResponse() With {.RepositoryInfo = repositoryInfo}
         End If
      End Function
      ''' <summary>
      ''' internal usage
      ''' </summary>
      Private Overloads Function GetRepositoryInfo(repositoryId As String, token As String) As Generic.Response(Of Core.cmisRepositoryInfoType)
         Dim request As New Messaging.Requests.getRepositoryInfo() With {.RepositoryId = repositoryId}

         request.BrowserBinding.Token = token
         Dim response = GetRepositoryInfo(request)

         If response.Exception Is Nothing Then
            Return response.Response.RepositoryInfo
         Else
            Return response.Exception
         End If
      End Function

      ''' <summary>
      ''' Returns the list of object-types defined for the repository that are children of the specified type
      ''' </summary>
      Public Overrides Function GetTypeChildren(request As Messaging.Requests.getTypeChildren) As Generic.Response(Of Messaging.Responses.getTypeChildrenResponse)
         request.BrowserBinding.CmisSelector = "typeChildren"
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim result = Me.Get(New UriBuilder(.Response.RepositoryUrl, request,
                                                  "typeId", request.TypeId, True,
                                                  "includePropertyDefinitions", request.IncludePropertyDefinitions,
                                                  "maxItems", request.MaxItems, "skipCount", request.SkipCount),
                                   AddressOf Deserialize(Of Messaging.cmisTypeDefinitionListType),
                                   False)
               If result.Exception Is Nothing Then
                  Return New Messaging.Responses.getTypeChildrenResponse() With {.Types = SetTypeDefinitions(result.Response, request.RepositoryId)}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Gets the definition of the specified object-type
      ''' </summary>
      Public Overrides Function GetTypeDefinition(request As Messaging.Requests.getTypeDefinition) As Generic.Response(Of Messaging.Responses.getTypeDefinitionResponse)
         request.BrowserBinding.CmisSelector = "typeDefinition"
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim typeId As String = request.TypeId
               Dim state As Vendors.Vendor.State = _vendor.BeginRequest(request.RepositoryId, typeId)
               Dim result = Me.Get(New UriBuilder(.Response.RepositoryUrl, request, "typeId", typeId, True),
                                   AddressOf Deserialize(Of Core.Definitions.Types.cmisTypeDefinitionType),
                                   False)
               If result.Exception Is Nothing Then
                  Dim type As Core.Definitions.Types.cmisTypeDefinitionType = result.Response

                  'cache typeDefinition
                  If type IsNot Nothing Then
                     Dim repositoryId As String = request.RepositoryId

                     If Not (String.IsNullOrEmpty(repositoryId) OrElse String.IsNullOrEmpty(typeId)) Then
                        TypeDefinition(repositoryId, typeId) = CType(type.Copy(), Core.Definitions.Types.cmisTypeDefinitionType)
                     End If
                  End If
                  _vendor.EndRequest(state, type)
                  Return New Messaging.Responses.getTypeDefinitionResponse() With {.Type = type}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Returns the set of the descendant object-types defined for the Repository under the specified type
      ''' </summary>
      Public Overrides Function GetTypeDescendants(request As Messaging.Requests.getTypeDescendants) As Generic.Response(Of Messaging.Responses.getTypeDescendantsResponse)
         request.BrowserBinding.CmisSelector = "typeDescendants"
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim typeId As String = request.TypeId
               Dim uriBuilder As New UriBuilder(.Response.RepositoryUrl, request, "typeId", typeId, "includePropertyDefinitions", request.IncludePropertyDefinitions)

               'if typeId is not defined then all types have to be returned (see 2.2.2.4.1 Inputs)
               If Not String.IsNullOrEmpty(typeId) Then uriBuilder.Add("depth", request.Depth)
               Dim result = Me.Get(uriBuilder.ToUri(),
                                   AddressOf DeserializeArray(Of Messaging.cmisTypeContainer),
                                   False)
               If result.Exception Is Nothing Then
                  Return New Messaging.Responses.getTypeDescendantsResponse() With {.Types = SetTypeDefinitions(result.Response, request.RepositoryId)}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Updates a type definition
      ''' </summary>
      Public Overrides Function UpdateType(request As Messaging.Requests.updateType) As Generic.Response(Of Messaging.Responses.updateTypeResponse)
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim content As New JSON.MultipartFormDataContent(MediaTypes.MultipartFormData, "updatetype")

               content.Add(request.Type)
               Dim result = Me.Post(New UriBuilder(.Response.RepositoryUrl, request), content, Nothing,
                                    AddressOf Deserialize(Of Core.Definitions.Types.cmisTypeDefinitionType),
                                    False)
               If result.Exception Is Nothing Then
                  Dim type As Core.Definitions.Types.cmisTypeDefinitionType = result.Response

                  'cache typeDefinition
                  If type IsNot Nothing Then
                     Dim typeId As String = type.Id
                     Dim repositoryId As String = request.RepositoryId

                     If Not (String.IsNullOrEmpty(repositoryId) OrElse String.IsNullOrEmpty(typeId)) Then
                        TypeDefinition(repositoryId, typeId) = type
                     End If
                  End If

                  Return New Messaging.Responses.updateTypeResponse() With {.Type = type}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function
#End Region

#Region "Navigation"
      ''' <summary>
      ''' Gets the list of documents that are checked out that the user has access to
      ''' </summary>
      Public Overrides Function GetCheckedOutDocs(request As Messaging.Requests.getCheckedOutDocs) As Generic.Response(Of Messaging.Responses.getCheckedOutDocsResponse)
         request.BrowserBinding.CmisSelector = "checkedOut"
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim uriBuilder As New UriBuilder(If(String.IsNullOrEmpty(request.FolderId), .Response.RepositoryUrl, .Response.RootFolderUrl), request)

               If Not String.IsNullOrEmpty(request.FolderId) Then uriBuilder.Add("objectId", request.FolderId)
               uriBuilder.AddRange("maxItems", request.MaxItems,
                                   "skipCount", request.SkipCount,
                                   "orderBy", request.OrderBy,
                                   "filter", request.Filter,
                                   "includeRelationships", request.IncludeRelationships,
                                   "renditionFilter", request.RenditionFilter,
                                   "includeAllowableActions", request.IncludeAllowableActions)
               Dim result = Me.Get(uriBuilder.ToUri(),
                                   AddressOf Deserialize(Of Messaging.Responses.getContentChangesResponse),
                                   request.BrowserBinding.Succinct)
               If result.Exception Is Nothing Then
                  TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                  Return New Messaging.Responses.getCheckedOutDocsResponse() With {.Objects = result.Response.Objects}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Gets the list of child objects contained in the specified folder
      ''' </summary>
      Public Overrides Function GetChildren(request As Messaging.Requests.getChildren) As Generic.Response(Of Messaging.Responses.getChildrenResponse)
         request.BrowserBinding.CmisSelector = "children"
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim result = Me.Get(New UriBuilder(.Response.RootFolderUrl, request,
                                                  "objectId", request.FolderId, True,
                                                  "maxItems", request.MaxItems,
                                                  "skipCount", request.SkipCount,
                                                  "orderBy", request.OrderBy,
                                                  "filter", request.Filter,
                                                  "includeRelationships", request.IncludeRelationships,
                                                  "renditionFilter", request.RenditionFilter,
                                                  "includeAllowableActions", request.IncludeAllowableActions,
                                                  "includePathSegment", request.IncludePathSegment),
                                   AddressOf Deserialize(Of Messaging.cmisObjectInFolderListType),
                                   request.BrowserBinding.Succinct)
               If result.Exception Is Nothing Then
                  TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                  Return New Messaging.Responses.getChildrenResponse() With {.Objects = result.Response}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Gets the set of descendant objects containded in the specified folder or any of its child-folders
      ''' </summary>
      Public Overrides Function GetDescendants(request As Messaging.Requests.getDescendants) As Generic.Response(Of Messaging.Responses.getDescendantsResponse)
         request.BrowserBinding.CmisSelector = "descendants"
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim result = Me.Get(New UriBuilder(.Response.RootFolderUrl, request,
                                                  "objectId", request.FolderId, True,
                                                  "depth", request.Depth,
                                                  "filter", request.Filter,
                                                  "includeAllowableActions", request.IncludeAllowableActions,
                                                  "includeRelationships", request.IncludeRelationships,
                                                  "renditionFilter", request.RenditionFilter,
                                                  "includePathSegment", request.IncludePathSegment),
                                   AddressOf DeserializeArray(Of Messaging.cmisObjectInFolderContainerType),
                                   request.BrowserBinding.Succinct)
               If result.Exception Is Nothing Then
                  TransformResponse(New Messaging.cmisObjectInFolderContainerType() With {.Children = result.Response}, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                  Return New Messaging.Responses.getDescendantsResponse() With {.Objects = result.Response}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Gets the parent folder object for the specified folder object
      ''' </summary>
      Public Overrides Function GetFolderParent(request As Messaging.Requests.getFolderParent) As Generic.Response(Of Messaging.Responses.getFolderParentResponse)
         request.BrowserBinding.CmisSelector = "parent"
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim result = Me.Get(New UriBuilder(.Response.RootFolderUrl, request,
                                                  "objectId", request.FolderId, True,
                                                  "filter", request.Filter),
                                   AddressOf Deserialize(Of Core.cmisObjectType),
                                   request.BrowserBinding.Succinct)
               If result.Exception Is Nothing Then
                  TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                  Return New Messaging.Responses.getFolderParentResponse() With {.Object = result.Response}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Gets the set of descendant folder objects contained in the specified folder
      ''' </summary>
      Public Overrides Function GetFolderTree(request As Messaging.Requests.getFolderTree) As Generic.Response(Of Messaging.Responses.getFolderTreeResponse)
         request.BrowserBinding.CmisSelector = "folderTree"
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim result = Me.Get(New UriBuilder(.Response.RootFolderUrl, request,
                                                  "objectId", request.FolderId, True,
                                                  "depth", request.Depth,
                                                  "filter", request.Filter,
                                                  "includeAllowableActions", request.IncludeAllowableActions,
                                                  "includeRelationships", request.IncludeRelationships,
                                                  "renditionFilter", request.RenditionFilter,
                                                  "includePathSegment", request.IncludePathSegment),
                                   AddressOf DeserializeArray(Of Messaging.cmisObjectInFolderContainerType),
                                   request.BrowserBinding.Succinct)
               If result.Exception Is Nothing Then
                  TransformResponse(New Messaging.cmisObjectInFolderContainerType() With {.Children = result.Response}, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                  Return New Messaging.Responses.getFolderTreeResponse() With {.Objects = result.Response}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Gets the parent folder(s) for the specified fileable object
      ''' </summary>
      Public Overrides Function GetObjectParents(request As Messaging.Requests.getObjectParents) As Generic.Response(Of Messaging.Responses.getObjectParentsResponse)
         request.BrowserBinding.CmisSelector = "parents"
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim result = Me.Get(New UriBuilder(.Response.RootFolderUrl, request,
                                                  "objectId", request.ObjectId, True,
                                                  "filter", request.Filter,
                                                  "includeRelationships", request.IncludeRelationships,
                                                  "renditionFilter", request.RenditionFilter,
                                                  "includeAllowableActions", request.IncludeAllowableActions,
                                                  "includeRelativePathSegment", request.IncludeRelativePathSegment),
                                   AddressOf DeserializeArray(Of Messaging.cmisObjectParentsType),
                                   request.BrowserBinding.Succinct)
               If result.Exception Is Nothing Then
                  Dim parents As IEnumerable(Of Core.cmisObjectType) = (From parent As Messaging.cmisObjectParentsType In If(result.Response, New Messaging.cmisObjectParentsType() {})
                                                                        Let parentObject As Core.cmisObjectType = If(parent Is Nothing, Nothing, parent.Object)
                                                                        Where parentObject IsNot Nothing
                                                                        Select parentObject)
                  TransformResponse(parents, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                  Return New Messaging.Responses.getObjectParentsResponse() With {.Parents = result.Response}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function
#End Region

#Region "Object"
      ''' <summary>
      ''' Appends to the content stream for the specified document object
      ''' </summary>
      Public Overrides Function AppendContentStream(request As Messaging.Requests.appendContentStream) As Generic.Response(Of Messaging.Responses.appendContentStreamResponse)
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim content As New JSON.MultipartFormDataContent(MediaTypes.MultipartFormData, "appendContent", request)

               AddToContent(content, Nothing, Nothing, Nothing, Nothing, request.ContentStream)
               If request.IsLastChunk.HasValue Then content.Add("isLastChunk", Convert(request.IsLastChunk.Value))
               If Not String.IsNullOrEmpty(request.ChangeToken) Then content.Add("changeToken", request.ChangeToken)
               Dim result = Me.Post(New UriBuilder(.Response.RootFolderUrl, request, "objectId", request.ObjectId, True), content, Nothing,
                                    AddressOf Deserialize(Of Core.cmisObjectType),
                                    request.BrowserBinding.Succinct)
               If result.Exception Is Nothing Then
                  TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                  Return New Messaging.Responses.appendContentStreamResponse() With {.Object = result.Response}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Updates properties and secondary types of one or more objects
      ''' </summary>
      Public Overrides Function BulkUpdateProperties(request As Messaging.Requests.bulkUpdateProperties) As Generic.Response(Of Messaging.Responses.bulkUpdatePropertiesResponse)
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim content As New JSON.MultipartFormDataContent(MediaTypes.UrlEncodedUTF8, "bulkUpdate", request)
               Dim state As Vendors.Vendor.State = TransformRequest(request.RepositoryId, request.BulkUpdateData.Properties)

               Try
                  With request.BulkUpdateData
                     'Properties
                     AddToContent(content, .Properties, Nothing, Nothing, Nothing)
                     'changeTokens, objectIds
                     If .ObjectIdAndChangeTokens IsNot Nothing Then
                        For Each objectIdAndChangeToken As Core.cmisObjectIdAndChangeTokenType In .ObjectIdAndChangeTokens
                           content.Add(objectIdAndChangeToken.ChangeToken, JSON.enumValueType.changeToken)
                           content.Add(objectIdAndChangeToken.Id, JSON.enumValueType.objectId)
                        Next
                     End If
                     'addSecondaryIds and removeSecondaryIds
                     For Each de As KeyValuePair(Of JSON.enumCollectionAction, String()) In New Dictionary(Of JSON.enumCollectionAction, String()) From {
                        {JSON.enumCollectionAction.add, .AddSecondaryTypeIds},
                        {JSON.enumCollectionAction.remove, .RemoveSecondaryTypeIds}}
                        If de.Value IsNot Nothing Then
                           For Each secondaryTypeId As String In de.Value
                              content.Add(secondaryTypeId, de.Key)
                           Next
                        End If
                     Next
                  End With
                  Dim result = Me.Post(New UriBuilder(.Response.RepositoryUrl, request), content, Nothing,
                                       AddressOf DeserializeArray(Of Core.cmisObjectIdAndChangeTokenType),
                                       request.BrowserBinding.Succinct)
                  If result.Exception Is Nothing Then
                     Return New Messaging.Responses.bulkUpdatePropertiesResponse() With {.ObjectIdAndChangeTokens = result.Response}
                  Else
                     Return result.Exception
                  End If
               Finally
                  If state IsNot Nothing Then state.Rollback()
               End Try
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Creates a document object of the specified type (given by the cmis:objectTypeId property) in the (optionally) specified location
      ''' </summary>
      Public Overrides Function CreateDocument(request As Messaging.Requests.createDocument) As Generic.Response(Of Messaging.Responses.createDocumentResponse)
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim content As New JSON.MultipartFormDataContent(MediaTypes.MultipartFormData, "createDocument", request)
               Dim uriBuilder As New UriBuilder(If(String.IsNullOrEmpty(request.FolderId), .Response.RepositoryUrl, .Response.RootFolderUrl), request)
               Dim state As Vendors.Vendor.State = TransformRequest(request.RepositoryId, request.Properties)

               Try
                  If Not String.IsNullOrEmpty(request.FolderId) Then uriBuilder.Add("objectId", request.FolderId)
                  'append cmis:contentStreamFileName, cmis:contentStreamLength and cmis:contentStreamMimeType if necessary
                  AddToContent(content, If(request.ContentStream Is Nothing, request.Properties, request.ContentStream.ExtendProperties(request.Properties)),
                               request.Policies, request.AddACEs, request.RemoveACEs, request.ContentStream, request.VersioningState)
                  Dim result = Me.Post(uriBuilder.ToUri(), content, Nothing,
                                       AddressOf Deserialize(Of Core.cmisObjectType),
                                       request.BrowserBinding.Succinct)
                  If result.Exception Is Nothing Then
                     TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                     Return New Messaging.Responses.createDocumentResponse() With {.Object = result.Response}
                  Else
                     Return result.Exception
                  End If
               Finally
                  If state IsNot Nothing Then state.Rollback()
               End Try
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Creates a document object as a copy of the given source document in the (optionally) specified location
      ''' </summary>
      ''' <remarks>In chapter 5.4.2.7 Action "createDocumentFromSource" the listed relevant CMIS controls contains "Content",
      ''' but there is no equivalent in chapter 2.2.4.2  createDocumentFromSource. Therefore content is ignored.</remarks>
      Public Overrides Function CreateDocumentFromSource(request As Messaging.Requests.createDocumentFromSource) As Generic.Response(Of Messaging.Responses.createDocumentFromSourceResponse)
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim content As New JSON.MultipartFormDataContent(MediaTypes.MultipartFormData, "createDocumentFromSource", request)
               Dim uriBuilder As New UriBuilder(If(String.IsNullOrEmpty(request.FolderId), .Response.RepositoryUrl, .Response.RootFolderUrl), request)
               Dim state As Vendors.Vendor.State = TransformRequest(request.RepositoryId, request.Properties)

               Try
                  If Not String.IsNullOrEmpty(request.FolderId) Then uriBuilder.Add("objectId", request.FolderId)
                  'append cmis:contentStreamFileName, cmis:contentStreamLength and cmis:contentStreamMimeType if necessary
                  AddToContent(content, request.Properties, request.Policies, request.AddACEs, request.RemoveACEs, Nothing, request.VersioningState)
                  content.Add("sourceId", request.SourceId)
                  Dim result = Me.Post(uriBuilder.ToUri(), content, Nothing,
                                       AddressOf Deserialize(Of Core.cmisObjectType),
                                       request.BrowserBinding.Succinct)
                  If result.Exception Is Nothing Then
                     TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                     Return New Messaging.Responses.createDocumentFromSourceResponse() With {.Object = result.Response}
                  Else
                     Return result.Exception
                  End If
               Finally
                  If state IsNot Nothing Then state.Rollback()
               End Try
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Creates a folder object of the specified type (given by the cmis:objectTypeId property) in the specified location
      ''' </summary>
      Public Overrides Function CreateFolder(request As Messaging.Requests.createFolder) As Generic.Response(Of Messaging.Responses.createFolderResponse)
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim content As New JSON.MultipartFormDataContent(MediaTypes.MultipartFormData, "createFolder", request)
               Dim state As Vendors.Vendor.State = TransformRequest(request.RepositoryId, request.Properties)

               Try
                  AddToContent(content, request.Properties, request.Policies, request.AddACEs, request.RemoveACEs)
                  Dim result = Me.Post(New UriBuilder(.Response.RootFolderUrl, request, "objectId", request.FolderId, True), content, Nothing,
                                       AddressOf Deserialize(Of Core.cmisObjectType),
                                       request.BrowserBinding.Succinct)
                  If result.Exception Is Nothing Then
                     TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                     Return New Messaging.Responses.createFolderResponse() With {.Object = result.Response}
                  Else
                     Return result.Exception
                  End If
               Finally
                  If state IsNot Nothing Then state.Rollback()
               End Try
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Creates a item object of the specified type (given by the cmis:objectTypeId property) in (optionally) the specified location
      ''' </summary>
      Public Overrides Function CreateItem(request As Messaging.Requests.createItem) As Generic.Response(Of Messaging.Responses.createItemResponse)
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim content As New JSON.MultipartFormDataContent(MediaTypes.UrlEncodedUTF8, "createItem", request)
               Dim uriBuilder As New UriBuilder(If(String.IsNullOrEmpty(request.FolderId), .Response.RepositoryUrl, .Response.RootFolderUrl), request)
               Dim state As Vendors.Vendor.State = TransformRequest(request.RepositoryId, request.Properties)

               Try
                  If Not String.IsNullOrEmpty(request.FolderId) Then uriBuilder.Add("objectId", request.FolderId)
                  AddToContent(content, request.Properties, request.Policies, request.AddACEs, request.RemoveACEs)
                  Dim result = Me.Post(uriBuilder.ToUri(), content, Nothing,
                                       AddressOf Deserialize(Of Core.cmisObjectType),
                                       request.BrowserBinding.Succinct)
                  If result.Exception Is Nothing Then
                     TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                     Return New Messaging.Responses.createItemResponse() With {.Object = result.Response}
                  Else
                     Return result.Exception
                  End If
               Finally
                  If state IsNot Nothing Then state.Rollback()
               End Try
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Creates a policy object of the specified type (given by the cmis:objectTypeId property) in the (optionally) specified location
      ''' </summary>
      Public Overrides Function CreatePolicy(request As Messaging.Requests.createPolicy) As Generic.Response(Of Messaging.Responses.createPolicyResponse)
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim content As New JSON.MultipartFormDataContent(MediaTypes.UrlEncodedUTF8, "createPolicy", request)
               Dim uriBuilder As New UriBuilder(If(String.IsNullOrEmpty(request.FolderId), .Response.RepositoryUrl, .Response.RootFolderUrl), request)
               Dim state As Vendors.Vendor.State = TransformRequest(request.RepositoryId, request.Properties)

               Try
                  If Not String.IsNullOrEmpty(request.FolderId) Then uriBuilder.Add("objectId", request.FolderId)
                  AddToContent(content, request.Properties, request.Policies, request.AddACEs, request.RemoveACEs)
                  Dim result = Me.Post(uriBuilder.ToUri(), content, Nothing,
                                       AddressOf Deserialize(Of Core.cmisObjectType),
                                       request.BrowserBinding.Succinct)
                  If result.Exception Is Nothing Then
                     TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                     Return New Messaging.Responses.createPolicyResponse() With {.Object = result.Response}
                  Else
                     Return result.Exception
                  End If
               Finally
                  If state IsNot Nothing Then state.Rollback()
               End Try
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Creates a relationship object of the specified type (given by the cmis:objectTypeId property)
      ''' </summary>
      Public Overrides Function CreateRelationship(request As Messaging.Requests.createRelationship) As Generic.Response(Of Messaging.Responses.createRelationshipResponse)
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim content As New JSON.MultipartFormDataContent(MediaTypes.UrlEncodedUTF8, "createRelationship", request)
               Dim state As Vendors.Vendor.State = TransformRequest(request.RepositoryId, request.Properties)

               Try
                  AddToContent(content, request.Properties, request.Policies, request.AddACEs, request.RemoveACEs)
                  Dim result = Me.Post(New UriBuilder(.Response.RepositoryUrl, request), content, Nothing,
                                       AddressOf Deserialize(Of Core.cmisObjectType),
                                       request.BrowserBinding.Succinct)
                  If result.Exception Is Nothing Then
                     TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                     Return New Messaging.Responses.createRelationshipResponse() With {.Object = result.Response}
                  Else
                     Return result.Exception
                  End If
               Finally
                  If state IsNot Nothing Then state.Rollback()
               End Try
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Deletes the content stream of a cmis document
      ''' </summary>
      Public Overrides Function DeleteContentStream(request As Messaging.Requests.deleteContentStream) As Generic.Response(Of Messaging.Responses.deleteContentStreamResponse)
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim content As New JSON.MultipartFormDataContent(MediaTypes.UrlEncodedUTF8, "deleteContent", request)

               content.Add("changeToken", request.ChangeToken)
               Dim result = Me.Post(New UriBuilder(.Response.RootFolderUrl, request, "objectId", request.ObjectId, True), content, Nothing,
                                    AddressOf Deserialize(Of Core.cmisObjectType),
                                    request.BrowserBinding.Succinct)
               If result.Exception Is Nothing Then
                  TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                  Return New Messaging.Responses.deleteContentStreamResponse() With {.Object = result.Response}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Deletes the specified object
      ''' </summary>
      Public Overrides Function DeleteObject(request As Messaging.Requests.deleteObject) As Generic.Response(Of Messaging.Responses.deleteObjectResponse)
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim e = EventBus.EventArgs.DispatchBeginEvent(Me, Nothing, ServiceDocUri.AbsoluteUri, request.RepositoryId, EventBus.enumBuiltInEvents.DeleteObject, request.ObjectId)
               Dim content As New JSON.MultipartFormDataContent(MediaTypes.UrlEncodedUTF8, "delete")

               If request.AllVersions.HasValue Then content.Add("allVersions", Convert(request.AllVersions.Value))
               Dim result = Me.Post(New UriBuilder(.Response.RootFolderUrl, request, "objectId", request.ObjectId, True), content, Nothing)
               If result.Exception Is Nothing Then
                  e.DispatchEndEvent(New Dictionary(Of String, Object)() From {{EventBus.EventArgs.PredefinedPropertyNames.Succeeded, True}})
                  Return New Messaging.Responses.deleteObjectResponse()
               Else
                  e.DispatchEndEvent(New Dictionary(Of String, Object)() From {{EventBus.EventArgs.PredefinedPropertyNames.Succeeded, False},
                                                                               {EventBus.EventArgs.PredefinedPropertyNames.Failure, result.Exception}})
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Deletes the specified folder object and all of its child- and descendant-objects
      ''' </summary>
      Public Overrides Function DeleteTree(request As Messaging.Requests.deleteTree) As Generic.Response(Of Messaging.Responses.deleteTreeResponse)
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim content As New JSON.MultipartFormDataContent(MediaTypes.UrlEncodedUTF8, "deleteTree")

               If request.AllVersions.HasValue Then content.Add("allVersions", Convert(request.AllVersions.Value))
               If request.UnfileObjects.HasValue Then content.Add("unfileObjects", request.UnfileObjects.Value.GetName())
               If request.ContinueOnFailure.HasValue Then content.Add("continueOnFailure", Convert(request.ContinueOnFailure.Value))
               Dim result = Me.Post(New UriBuilder(.Response.RootFolderUrl, request, "objectId", request.FolderId, True), content, Nothing,
                                    AddressOf Deserialize(Of Core.Collections.cmisListOfIdsType),
                                    request.BrowserBinding.Succinct)
               If result Is Nothing OrElse result.Exception Is Nothing Then
                  Return New Messaging.Responses.deleteTreeResponse() With {.FailedToDelete = If(result Is Nothing OrElse result.Response Is Nothing OrElse result.Response.Ids Is Nothing,
                                                                                                 Nothing, New Messaging.failedToDelete(result.Response.Ids))}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Gets the list of allowable actions for an object
      ''' </summary>
      Public Overrides Function GetAllowableActions(request As Messaging.Requests.getAllowableActions) As Generic.Response(Of Messaging.Responses.getAllowableActionsResponse)
         request.BrowserBinding.CmisSelector = "allowableActions"
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim result = Me.Get(New UriBuilder(.Response.RootFolderUrl, request, "objectId", request.ObjectId, True),
                                   AddressOf Deserialize(Of Core.cmisAllowableActionsType),
                                   request.BrowserBinding.Succinct)
               If result.Exception Is Nothing Then
                  Return New Messaging.Responses.getAllowableActionsResponse() With {.AllowableActions = result.Response}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Gets the content stream for the specified document object, or gets a rendition stream for a specified rendition of a document or folder object.
      ''' </summary>
      Public Overrides Function GetContentStream(request As Messaging.Requests.getContentStream) As Generic.Response(Of Messaging.Responses.getContentStreamResponse)
         request.BrowserBinding.CmisSelector = "content"
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
#If HttpRequestAddRangeShortened Then
               'Range properties (offset, length) of a HttpWebRequest are defined as Integer (see System.Net.HttpWebRequest-class),
               'therefore the xs_Integer has to be transformed
               Dim offset As Integer?
               Dim length As Integer?

               If request.Offset.HasValue Then
                  offset = CInt(request.Offset.Value)
               Else
                  offset = Nothing
               End If
               If request.Length.HasValue Then
                  length = CInt(request.Length.Value)
               Else
                  length = Nothing
               End If

               Dim result = Me.Get(New UriBuilder(.Response.RootFolderUrl, request,
                                                  "objectId", request.ObjectId, True,
                                                  "streamId", request.StreamId),
                                   offset, length)
#Else
               Dim result = Me.Get(New UriBuilder(.Response.RootFolderUrl, request,
                                                  "objectId", request.ObjectId, True,
                                                  "streamId", request.StreamId),
                                   request.Offset, request.Length)
#End If
               If result.Exception Is Nothing Then
                  'Maybe the filename is sent via Content-Disposition
                  Dim headers As System.Net.WebHeaderCollection = If(result.WebResponse Is Nothing, Nothing, result.WebResponse.Headers)
                  Dim disposition As String = Nothing
                  Dim fileName As String = If(headers Is Nothing, Nothing,
                                              RFC2231Helper.DecodeContentDisposition(headers(RFC2231Helper.ContentDispositionHeaderName), disposition))

                  Return New Messaging.Responses.getContentStreamResponse() With {.ContentStream = New cm.cmisContentStreamType(result.Stream, fileName, result.ContentType)}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Returns the uri to get the content of a cmisDocument
      ''' </summary>
      Public Overrides Function GetContentStreamLink(repositoryId As String, objectId As String, Optional streamId As String = Nothing) As Generic.Response(Of String)
         Dim request As New Messaging.Requests.getContentStream()

         request.BrowserBinding.CmisSelector = "content"
         request.ObjectId = objectId
         request.RepositoryId = repositoryId
         If Not streamId Is Nothing Then request.StreamId = streamId
         With GetRepositoryInfo(repositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim uriBuilder As New UriBuilder(.Response.RootFolderUrl, request,
                                                "objectId", objectId, True,
                                                "streamId", streamId)
               Return uriBuilder.ToUri().AbsoluteUri()
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Gets the specified information for the object
      ''' </summary>
      Public Overrides Function GetObject(request As Messaging.Requests.getObject) As Generic.Response(Of Messaging.Responses.getObjectResponse)
         request.BrowserBinding.CmisSelector = "object"
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim result = Me.Get(New UriBuilder(.Response.RootFolderUrl, request,
                                                  "objectId", request.ObjectId, True,
                                                  "filter", request.Filter,
                                                  "includeRelationships", request.IncludeRelationships,
                                                  "includePolicyIds", request.IncludePolicyIds,
                                                  "renditionFilter", request.RenditionFilter,
                                                  "includeACL", request.IncludeACL,
                                                  "includeAllowableActions", request.IncludeAllowableActions),
                                   AddressOf Deserialize(Of Core.cmisObjectType),
                                   request.BrowserBinding.Succinct)
               If result.Exception Is Nothing Then
                  TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                  Return New Messaging.Responses.getObjectResponse() With {.Object = result.Response}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Gets the specified information for the object
      ''' </summary>
      Public Overrides Function GetObjectByPath(request As Messaging.Requests.getObjectByPath) As Generic.Response(Of Messaging.Responses.getObjectByPathResponse)
         request.BrowserBinding.CmisSelector = "object"
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim result = Me.Get(New UriBuilder(CreateObjectUri(.Response.RootFolderUrl, request.Path), request,
                                                  "filter", request.Filter,
                                                  "includeRelationships", request.IncludeRelationships,
                                                  "includePolicyIds", request.IncludePolicyIds,
                                                  "renditionFilter", request.RenditionFilter,
                                                  "includeACL", request.IncludeACL,
                                                  "includeAllowableActions", request.IncludeAllowableActions),
                                   AddressOf Deserialize(Of Core.cmisObjectType),
                                   request.BrowserBinding.Succinct)
               If result.Exception Is Nothing Then
                  TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                  Return New Messaging.Responses.getObjectByPathResponse() With {.Object = result.Response}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Gets the list of properties for the object
      ''' </summary>
      Public Overrides Function GetProperties(request As Messaging.Requests.getProperties) As Generic.Response(Of Messaging.Responses.getPropertiesResponse)
         request.BrowserBinding.CmisSelector = "properties"
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim result = Me.Get(New UriBuilder(.Response.RootFolderUrl, request,
                                                  "objectId", request.ObjectId, True,
                                                  "filter", request.Filter),
                                   AddressOf Deserialize(Of Core.Collections.cmisPropertiesType),
                                   request.BrowserBinding.Succinct)
               If result.Exception Is Nothing Then
                  TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                  Return New Messaging.Responses.getPropertiesResponse() With {.Properties = result.Response}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Gets the list of associated renditions for the specified object. Only rendition attributes are returned, not rendition stream
      ''' </summary>
      Public Overrides Function GetRenditions(request As Messaging.Requests.getRenditions) As Generic.Response(Of Messaging.Responses.getRenditionsResponse)
         request.BrowserBinding.CmisSelector = "renditions"
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim result = Me.Get(New UriBuilder(.Response.RootFolderUrl, request,
                                                  "objectId", request.ObjectId, True,
                                                  "renditionFilter", request.RenditionFilter,
                                                  "maxItems", request.MaxItems,
                                                  "skipCount", request.SkipCount),
                                   AddressOf DeserializeArray(Of Core.cmisRenditionType),
                                   request.BrowserBinding.Succinct)
               If result.Exception Is Nothing Then
                  Return New Messaging.Responses.getRenditionsResponse() With {.Renditions = result.Response}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Moves the specified file-able object from one folder to another
      ''' </summary>
      Public Overrides Function MoveObject(request As Messaging.Requests.moveObject) As Generic.Response(Of Messaging.Responses.moveObjectResponse)
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim content As New JSON.MultipartFormDataContent(MediaTypes.UrlEncodedUTF8, "move", request)

               content.Add("sourceFolderId", request.SourceFolderId)
               content.Add("targetFolderId", request.TargetFolderId)
               Dim result = Me.Post(New UriBuilder(.Response.RootFolderUrl, request, "objectId", request.ObjectId, True), content, Nothing,
                                    AddressOf Deserialize(Of Core.cmisObjectType),
                                    request.BrowserBinding.Succinct)
               If result.Exception Is Nothing Then
                  TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                  Return New Messaging.Responses.moveObjectResponse() With {.Object = result.Response}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Sets the content stream for the specified document object
      ''' </summary>
      Public Overrides Function SetContentStream(request As Messaging.Requests.setContentStream) As Generic.Response(Of Messaging.Responses.setContentStreamResponse)
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim content As New JSON.MultipartFormDataContent(MediaTypes.MultipartFormData, "setContent", request)

               content.Add("changeToken", request.ChangeToken)
               If request.OverwriteFlag.HasValue Then content.Add("overwriteFlag", Convert(request.OverwriteFlag.Value))
               AddToContent(content, Nothing, Nothing, Nothing, Nothing, request.ContentStream)
               Dim result = Me.Post(New UriBuilder(.Response.RootFolderUrl, request, "objectId", request.ObjectId, True), content, Nothing,
                                    AddressOf Deserialize(Of Core.cmisObjectType),
                                    request.BrowserBinding.Succinct)
               If result.Exception Is Nothing Then
                  TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                  Return New Messaging.Responses.setContentStreamResponse() With {.Object = result.Response}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Updates properties and secondary types of the specified object
      ''' </summary>
      Public Overrides Function UpdateProperties(request As Messaging.Requests.updateProperties) As Generic.Response(Of Messaging.Responses.updatePropertiesResponse)
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim content As New JSON.MultipartFormDataContent(MediaTypes.MultipartFormData, "update", request)
               Dim state As Vendors.Vendor.State = TransformRequest(request.RepositoryId, request.Properties)

               Try
                  content.Add("changeToken", request.ChangeToken)
                  AddToContent(content, request.Properties, Nothing, Nothing, Nothing)
                  Dim result = Me.Post(New UriBuilder(.Response.RootFolderUrl, request, "objectId", request.ObjectId, True), content, Nothing,
                                       AddressOf Deserialize(Of Core.cmisObjectType),
                                       request.BrowserBinding.Succinct)
                  If result.Exception Is Nothing Then
                     TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                     Return New Messaging.Responses.updatePropertiesResponse() With {.Object = result.Response}
                  Else
                     Return result.Exception
                  End If
               Finally
                  If state IsNot Nothing Then state.Rollback()
               End Try
            Else
               Return .Exception
            End If
         End With
      End Function
#End Region

#Region "Multi"
      ''' <summary>
      ''' Adds an existing fileable non-folder object to a folder
      ''' </summary>
      Public Overrides Function AddObjectToFolder(request As Messaging.Requests.addObjectToFolder) As Generic.Response(Of Messaging.Responses.addObjectToFolderResponse)
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim content As New JSON.MultipartFormDataContent(MediaTypes.UrlEncodedUTF8, "addObjectToFolder", request)

               content.Add("folderId", request.FolderId)
               If request.AllVersions.HasValue Then content.Add("allVersions", Convert(request.AllVersions.Value))
               Dim result = Me.Post(New UriBuilder(.Response.RootFolderUrl, request, "objectId", request.ObjectId, True), content, Nothing,
                                    AddressOf Deserialize(Of Core.cmisObjectType),
                                    request.BrowserBinding.Succinct)
               If result.Exception Is Nothing Then
                  TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                  Return New Messaging.Responses.addObjectToFolderResponse() With {.Object = result.Response}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Removes an existing fileable non-folder object from a folder
      ''' </summary>
      Public Overrides Function RemoveObjectFromFolder(request As Messaging.Requests.removeObjectFromFolder) As Generic.Response(Of Messaging.Responses.removeObjectFromFolderResponse)
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim content As New JSON.MultipartFormDataContent(MediaTypes.UrlEncodedUTF8, "removeObjectFromFolder", request)

               content.Add("folderId", request.FolderId)
               Dim result = Me.Post(New UriBuilder(.Response.RootFolderUrl, request, "objectId", request.ObjectId, True), content, Nothing,
                                    AddressOf Deserialize(Of Core.cmisObjectType),
                                    request.BrowserBinding.Succinct)
               If result.Exception Is Nothing Then
                  TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                  Return New Messaging.Responses.removeObjectFromFolderResponse() With {.Object = result.Response}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function
#End Region

#Region "Disc"
      ''' <summary>
      ''' Gets a list of content changes. This service is intended to be used by search crawlers or other applications that need to
      ''' efficiently understand what has changed in the repository
      ''' </summary>
      Public Overrides Function GetContentChanges(request As Messaging.Requests.getContentChanges) As Generic.Response(Of Messaging.Responses.getContentChangesResponse)
         request.BrowserBinding.CmisSelector = "contentChanges"
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim result = Me.Get(New UriBuilder(.Response.RepositoryUrl, request,
                                                  "filter", request.Filter,
                                                  "changeLogToken", request.ChangeLogToken,
                                                  "includeProperties", request.IncludeProperties,
                                                  "includePolicyIds", request.IncludePolicyIds,
                                                  "includeACL", request.IncludeACL,
                                                  "maxItems", request.MaxItems),
                                   AddressOf Deserialize(Of Messaging.Responses.getContentChangesResponse),
                                   request.BrowserBinding.Succinct)
               If result.Exception Is Nothing Then
                  TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                  Return result
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Executes a CMIS query statement against the contents of the repository
      ''' </summary>
      ''' <remarks>Another way to implement this method is to use a POST-request</remarks>
      Public Overrides Function Query(request As Messaging.Requests.query) As Generic.Response(Of Messaging.Responses.queryResponse)
         request.BrowserBinding.CmisSelector = "query"
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim result = Me.Get(New UriBuilder(.Response.RepositoryUrl, request,
                                                  "q", request.Statement, True,
                                                  "searchAllVersions", request.SearchAllVersions,
                                                  "maxItems", request.MaxItems,
                                                  "skipCount", request.SkipCount,
                                                  "includeAllowableActions", request.IncludeAllowableActions,
                                                  "includeRelationships", request.IncludeRelationships,
                                                  "renditionFilter", request.RenditionFilter),
                                   AddressOf Deserialize(Of Messaging.cmisObjectListType),
                                   request.BrowserBinding.Succinct)
               If result.Exception Is Nothing Then
                  TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                  Return New Messaging.Responses.queryResponse() With {.Objects = result.Response}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function
#End Region

#Region "Versioning"
      ''' <summary>
      ''' Reverses the effect of a check-out (checkOut). Removes the Private Working Copy of the checked-out document, allowing other documents
      ''' in the version series to be checked out again. If the private working copy has been created by createDocument, cancelCheckOut MUST
      ''' delete the created document
      ''' </summary>
      Public Overrides Function CancelCheckOut(request As Messaging.Requests.cancelCheckOut) As Generic.Response(Of Messaging.Responses.cancelCheckOutResponse)
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim e = EventBus.EventArgs.DispatchBeginEvent(Me, Nothing, ServiceDocUri.AbsoluteUri, request.RepositoryId, EventBus.enumBuiltInEvents.CancelCheckout, request.ObjectId)
               Dim content As New JSON.MultipartFormDataContent(MediaTypes.UrlEncodedUTF8, "cancelCheckOut", request)
               Dim result = Me.Post(New UriBuilder(.Response.RootFolderUrl, request, "objectId", request.ObjectId, True), content, Nothing)
               If result.Exception Is Nothing Then
                  e.DispatchEndEvent(New Dictionary(Of String, Object)() From {{EventBus.EventArgs.PredefinedPropertyNames.Succeeded, True}})
                  Return New Messaging.Responses.cancelCheckOutResponse()
               Else
                  e.DispatchEndEvent(New Dictionary(Of String, Object)() From {{EventBus.EventArgs.PredefinedPropertyNames.Succeeded, False},
                                                                               {EventBus.EventArgs.PredefinedPropertyNames.Failure, result.Exception}})
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Checks-in the Private Working Copy document.
      ''' </summary>
      Public Overrides Function CheckIn(request As Messaging.Requests.checkIn) As Generic.Response(Of Messaging.Responses.checkInResponse)
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim e = EventBus.EventArgs.DispatchBeginEvent(Me, Nothing, ServiceDocUri.AbsoluteUri, request.RepositoryId, EventBus.enumBuiltInEvents.CheckIn, request.ObjectId)
               Dim content As New JSON.MultipartFormDataContent(MediaTypes.MultipartFormData, "checkIn", request)
               Dim state As Vendors.Vendor.State = TransformRequest(request.RepositoryId, request.Properties)

               Try
                  AddToContent(content, request.Properties, request.Policies, request.AddACEs, request.RemoveACEs, request.ContentStream)
                  If request.Major.HasValue Then content.Add("major", Convert(request.Major.Value))
                  content.Add("checkinComment", request.CheckinComment)
                  Dim result = Me.Post(New UriBuilder(.Response.RootFolderUrl, request, "objectId", request.ObjectId, True), content, Nothing,
                                       AddressOf Deserialize(Of Core.cmisObjectType),
                                       request.BrowserBinding.Succinct)
                  If result.Exception Is Nothing Then
                     TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                     e.DispatchEndEvent(New Dictionary(Of String, Object)() From {{EventBus.EventArgs.PredefinedPropertyNames.Succeeded, True},
                                                                                  {EventBus.EventArgs.PredefinedPropertyNames.NewObjectId, result.Response.ObjectId.Value}})
                     Return New Messaging.Responses.checkInResponse() With {.Object = result.Response}
                  Else
                     e.DispatchEndEvent(New Dictionary(Of String, Object)() From {{EventBus.EventArgs.PredefinedPropertyNames.Succeeded, False},
                                                                                  {EventBus.EventArgs.PredefinedPropertyNames.Failure, result.Exception}})
                     Return result.Exception
                  End If
               Finally
                  If state IsNot Nothing Then state.Rollback()
               End Try
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Checks out the specified CMIS object.
      ''' </summary>
      Public Overrides Function CheckOut(request As Messaging.Requests.checkOut) As Generic.Response(Of Messaging.Responses.checkOutResponse)
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim content As New JSON.MultipartFormDataContent(MediaTypes.UrlEncodedUTF8, "checkOut", request)
               Dim result = Me.Post(New UriBuilder(.Response.RootFolderUrl, request, "objectId", request.ObjectId, True), content, Nothing,
                                    AddressOf Deserialize(Of Core.cmisObjectType),
                                    request.BrowserBinding.Succinct)
               If result.Exception Is Nothing Then
                  TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                  Return New Messaging.Responses.checkOutResponse() With {.Object = result.Response}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Returns the list of all document objects in the specified version series, sorted by cmis:creationDate descending
      ''' </summary>
      Public Overrides Function GetAllVersions(request As Messaging.Requests.getAllVersions) As Generic.Response(Of Messaging.Responses.getAllVersionsResponse)
         request.BrowserBinding.CmisSelector = "versions"
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim result = Me.Get(New UriBuilder(.Response.RootFolderUrl, request, "objectId", request.ObjectId, True,
                                                  "filter", request.Filter,
                                                  "includeAllowableActions", request.IncludeAllowableActions),
                                   AddressOf DeserializeArray(Of Core.cmisObjectType),
                                   request.BrowserBinding.Succinct)
               If result.Exception Is Nothing Then
                  TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                  Return New Messaging.Responses.getAllVersionsResponse() With {.Objects = result.Response}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Get the latest document object in the version series
      ''' </summary>
      Public Overrides Function GetObjectOfLatestVersion(request As Messaging.Requests.getObjectOfLatestVersion) As Generic.Response(Of Messaging.Responses.getObjectOfLatestVersionResponse)
         request.BrowserBinding.CmisSelector = "object"
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim returnVersion As RestAtom.enumReturnVersion? = If(Not request.Major.HasValue, Nothing, If(request.Major.Value, RestAtom.enumReturnVersion.latestmajor, RestAtom.enumReturnVersion.latest))
               Dim result = Me.Get(New UriBuilder(.Response.RootFolderUrl, request,
                                                  "objectId", request.ObjectId, True,
                                                  "filter", request.Filter,
                                                  "includeRelationships", request.IncludeRelationships,
                                                  "includePolicyIds", request.IncludePolicyIds,
                                                  "renditionFilter", request.RenditionFilter,
                                                  "includeACL", request.IncludeACL,
                                                  "includeAllowableActions", request.IncludeAllowableActions,
                                                  "returnVersion", returnVersion),
                                   AddressOf Deserialize(Of Core.cmisObjectType),
                                   request.BrowserBinding.Succinct)
               If result.Exception Is Nothing Then
                  TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                  Return New Messaging.Responses.getObjectOfLatestVersionResponse() With {.Object = result.Response}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Get a subset of the properties for the latest document object in the version series
      ''' </summary>
      Public Overrides Function GetPropertiesOfLatestVersion(request As Messaging.Requests.getPropertiesOfLatestVersion) As Generic.Response(Of Messaging.Responses.getPropertiesOfLatestVersionResponse)
         request.BrowserBinding.CmisSelector = "properties"
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim returnVersion As RestAtom.enumReturnVersion? = If(Not request.Major.HasValue, Nothing, If(request.Major.Value, RestAtom.enumReturnVersion.latestmajor, RestAtom.enumReturnVersion.latest))
               Dim result = Me.Get(New UriBuilder(.Response.RootFolderUrl, request,
                                                  "objectId", request.ObjectId, True,
                                                  "filter", request.Filter,
                                                  "returnVersion", returnVersion),
                                   AddressOf Deserialize(Of Core.Collections.cmisPropertiesType),
                                   request.BrowserBinding.Succinct)
               If result.Exception Is Nothing Then
                  TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                  Return New Messaging.Responses.getPropertiesOfLatestVersionResponse() With {.Properties = result.Response}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function
#End Region

#Region "Rel"
      ''' <summary>
      ''' Gets all or a subset of relationships associated with an independent object
      ''' </summary>
      Public Overrides Function GetObjectRelationships(request As Messaging.Requests.getObjectRelationships) As Generic.Response(Of Messaging.Responses.getObjectRelationshipsResponse)
         request.BrowserBinding.CmisSelector = "relationships"
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim result = Me.Get(New UriBuilder(.Response.RootFolderUrl, request,
                                                  "objectId", request.ObjectId, True,
                                                  "includeSubRelationshipTypes", request.IncludeSubRelationshipTypes,
                                                  "relationshipDirection", request.RelationshipDirection,
                                                  "typeId", request.TypeId,
                                                  "maxItems", request.MaxItems,
                                                  "skipCount", request.SkipCount,
                                                  "filter", request.Filter,
                                                  "includeAllowableActions", request.IncludeAllowableActions),
                                   AddressOf Deserialize(Of Messaging.Responses.getContentChangesResponse),
                                   request.BrowserBinding.Succinct)
               If result.Exception Is Nothing Then
                  TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                  Return New Messaging.Responses.getObjectRelationshipsResponse() With {.Objects = result.Response.Objects}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function
#End Region

#Region "Policy"
      ''' <summary>
      ''' Applies a specified policy to an object
      ''' </summary>
      ''' <param name="request"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overrides Function ApplyPolicy(request As Messaging.Requests.applyPolicy) As Generic.Response(Of Messaging.Responses.applyPolicyResponse)
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim content As New JSON.MultipartFormDataContent(MediaTypes.UrlEncodedUTF8, "applyPolicy", request)

               content.Add("policyId", request.PolicyId)
               Dim result = Me.Post(New UriBuilder(.Response.RootFolderUrl, request, "objectId", request.ObjectId, True), content, Nothing,
                                    AddressOf Deserialize(Of Core.cmisObjectType),
                                    request.BrowserBinding.Succinct)
               If result.Exception Is Nothing Then
                  TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                  Return New Messaging.Responses.applyPolicyResponse() With {.Object = result.Response}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Gets the list of policies currently applied to the specified object
      ''' </summary>
      Public Overrides Function GetAppliedPolicies(request As Messaging.Requests.getAppliedPolicies) As Generic.Response(Of Messaging.Responses.getAppliedPoliciesResponse)
         request.BrowserBinding.CmisSelector = "policies"
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim result = Me.Get(New UriBuilder(.Response.RootFolderUrl, request,
                                                  "objectId", request.ObjectId, True,
                                                  "filter", request.Filter),
                                   AddressOf DeserializeArray(Of Core.cmisObjectType),
                                   request.BrowserBinding.Succinct)
               If result.Exception Is Nothing Then
                  TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                  Return New Messaging.Responses.getAppliedPoliciesResponse() With {.Objects = result.Response}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Removes a specified policy from an object
      ''' </summary>
      Public Overrides Function RemovePolicy(request As Messaging.Requests.removePolicy) As Generic.Response(Of Messaging.Responses.removePolicyResponse)
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim content As New JSON.MultipartFormDataContent(MediaTypes.UrlEncodedUTF8, "removePolicy", request)

               content.Add("policyId", request.PolicyId)
               Dim result = Me.Post(New UriBuilder(.Response.RootFolderUrl, request, "objectId", request.ObjectId, True), content, Nothing,
                                    AddressOf Deserialize(Of Core.cmisObjectType),
                                    request.BrowserBinding.Succinct)
               If result.Exception Is Nothing Then
                  TransformResponse(result.Response, New State(request.RepositoryId, request.BrowserBinding.Succinct))
                  Return New Messaging.Responses.removePolicyResponse() With {.Object = result.Response}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function
#End Region

#Region "ACL"
      ''' <summary>
      ''' Adds or removes the given ACEs to or from the ACL of an object
      ''' </summary>
      Public Overrides Function ApplyAcl(request As Messaging.Requests.applyACL) As Generic.Response(Of Messaging.Responses.applyACLResponse)
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim content As New JSON.MultipartFormDataContent(MediaTypes.UrlEncodedUTF8, "applyACL", request)

               AddToContent(content, Nothing, Nothing, request.AddACEs, request.RemoveACEs)
               If request.ACLPropagation.HasValue Then content.Add(request.ACLPropagation.Value)
               Dim result = Me.Post(New UriBuilder(.Response.RootFolderUrl, request, "objectId", request.ObjectId, True), content, Nothing,
                                    AddressOf Deserialize(Of Core.Security.cmisAccessControlListType),
                                    request.BrowserBinding.Succinct)
               If result.Exception Is Nothing Then
                  Return New Messaging.Responses.applyACLResponse() With {.ACL = New Messaging.cmisACLType() With {.ACL = result.Response,
                                                                                                                   .Exact = If(result.Response Is Nothing, Nothing, result.Response.IsExact)}}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function

      ''' <summary>
      ''' Get the ACL currently applied to the specified object
      ''' </summary>
      Public Overrides Function GetAcl(request As Messaging.Requests.getACL) As Generic.Response(Of Messaging.Responses.getACLResponse)
         request.BrowserBinding.CmisSelector = "acl"
         With GetRepositoryInfo(request.RepositoryId, request.BrowserBinding.Token)
            If .Exception Is Nothing Then
               Dim result = Me.Get(New UriBuilder(.Response.RootFolderUrl, request,
                                                  "objectId", request.ObjectId, True,
                                                  "onlyBasicPermissions", request.OnlyBasicPermissions),
                                   AddressOf Deserialize(Of Core.Security.cmisAccessControlListType),
                                   request.BrowserBinding.Succinct)
               If result.Exception Is Nothing Then
                  Return New Messaging.Responses.getACLResponse() With {.ACL = New Messaging.cmisACLType() With {.ACL = result.Response,
                                                                                                                 .Exact = If(result.Response Is Nothing, Nothing, result.Response.IsExact)}}
               Else
                  Return result.Exception
               End If
            Else
               Return .Exception
            End If
         End With
      End Function
#End Region

#Region "Miscellaneous (ICmisClient)"
      Public Overrides ReadOnly Property ClientType As enumClientType
         Get
            Return enumClientType.BrowserBinding
         End Get
      End Property

      ''' <summary>
      ''' Logs out from repository
      ''' </summary>
      Public Overrides Sub Logout(repositoryId As String)
         Dim content As New JSON.MultipartFormDataContent(MediaTypes.UrlEncodedUTF8, "logout")
         Dim uri As New Uri(ServiceURIs.GetServiceUri(_serviceDocUri.OriginalString,
                                                      ServiceURIs.enumRepositoriesUri.repositoryId Or ServiceURIs.enumRepositoriesUri.logout).ReplaceUri("repositoryId", repositoryId, "logout", "true"))
         Me.Post(uri, content, Nothing)
         RepositoryInfo(repositoryId) = Nothing
      End Sub

      ''' <summary>
      ''' Tells the server, that this client is still alive
      ''' </summary>
      ''' <remarks></remarks>
      Public Overrides Sub Ping(repositoryId As String)
         Dim content As New JSON.MultipartFormDataContent(MediaTypes.UrlEncodedUTF8, "ping")
         Dim uri As New Uri(ServiceURIs.GetServiceUri(_serviceDocUri.OriginalString,
                                                      ServiceURIs.enumRepositoriesUri.repositoryId Or ServiceURIs.enumRepositoriesUri.ping).ReplaceUri("repositoryId", repositoryId, "ping", "true"))
         Me.Post(uri, content, Nothing)
      End Sub

      Public Overrides ReadOnly Property SupportsSuccinct As Boolean
         Get
            Return True
         End Get
      End Property
      Public Overrides ReadOnly Property SupportsToken As Boolean
         Get
            Return True
         End Get
      End Property

      ''' <summary>
      ''' UserAgent-name of current instance
      ''' </summary>
      Protected Overrides ReadOnly Property UserAgent As String
         Get
            Return "Brügmann Software CmisObjectModel.Client.BrowserBinding.CmisClient"
         End Get
      End Property
#End Region

#Region "Requests"

#Region "Vendor specific and value mapping"
      ''' <summary>
      ''' Executes defined value mappings and processes vendor specific presentation of property values on all cmisObjects in objects
      ''' </summary>
      Private Shadows Sub TransformResponse(objects As IEnumerable(Of Core.cmisObjectType), state As State)
         If objects IsNot Nothing Then
            Dim repositoryId = state.RepositoryId
            Dim propertyCollections As Core.Collections.cmisPropertiesType() =
               (From cmisObject As Core.cmisObjectType In objects
                Let propertyCollection As Core.Collections.cmisPropertiesType = If(cmisObject Is Nothing, Nothing, cmisObject.Properties)
                Where propertyCollection IsNot Nothing
                Select propertyCollection).ToArray()

            MyBase.TransformResponse(state, state.TransformResponse(propertyCollections))
         End If
      End Sub
      ''' <summary>
      ''' Executes defined value mappings and processes vendor specific presentation of property values on a single cmisObject
      ''' </summary>
      Private Shadows Sub TransformResponse(cmisObject As Core.cmisObjectType, state As State)
         If cmisObject IsNot Nothing AndAlso cmisObject.Properties IsNot Nothing Then
            MyBase.TransformResponse(state, state.TransformResponse(New Core.Collections.cmisPropertiesType() {cmisObject.Properties}))
         End If
      End Sub
      ''' <summary>
      ''' Executes defined value mappings and processes vendor specific presentation of property values on cmisPropertiesType
      ''' </summary>
      Private Shadows Sub TransformResponse(properties As Core.Collections.cmisPropertiesType, state As State)
         If properties IsNot Nothing Then
            MyBase.TransformResponse(state, state.TransformResponse(New Core.Collections.cmisPropertiesType() {properties}))
         End If
      End Sub
#End Region

#Region "Get-Requests"
#If HttpRequestAddRangeShortened Then
      Private Function [Get](uri As Uri,
                             Optional offset As Integer? = Nothing, Optional length As Integer? = Nothing) As Response
#Else
      Private Function [Get](uri As Uri,
                             Optional offset As xs_Integer? = Nothing, Optional length As xs_Integer? = Nothing) As Response
#End If
         Try
            Return New Response(GetResponse(uri, "GET", Nothing, Nothing, Nothing, offset, length))
         Catch ex As sn.WebException
            Return New Response(ex)
         End Try
      End Function

#If HttpRequestAddRangeShortened Then
      Private Function [Get](Of TResponse)(uri As Uri, responseFactory As Func(Of String, String, Boolean, TResponse), succinct As Boolean,
                                           Optional offset As Integer? = Nothing, Optional length As Integer? = Nothing) As Generic.Response(Of TResponse)
#Else
      Private Function [Get](Of TResponse)(uri As Uri, responseFactory As Func(Of String, String, Boolean, TResponse), succinct As Boolean,
                                           Optional offset As xs_Integer? = Nothing, Optional length As xs_Integer? = Nothing) As Generic.Response(Of TResponse)
#End If
         Try
            Return New Generic.Response(Of TResponse)(GetResponse(uri, "GET", Nothing, Nothing, Nothing, offset, length),
                                                      Function(input As String, contentType As String) responseFactory(input, contentType, succinct))
         Catch ex As sn.WebException
            Return New Generic.Response(Of TResponse)(ex)
         End Try
      End Function
#End Region

#Region "Post-Requests"
      Private Function Post(uri As Uri, content As JSON.MultipartFormDataContent, headers As Dictionary(Of String, String)) As Response
         Dim contentWriter As Action(Of IO.Stream) = If(content Is Nothing, Nothing,
                                                        New Action(Of IO.Stream)(Sub(requestStream As IO.Stream)
                                                                                    content.WriteTo(requestStream)
                                                                                 End Sub))
         Try
            Return New Response(GetResponse(uri, "POST", contentWriter, content.ContentType, headers, Nothing, Nothing))
         Catch ex As sn.WebException
            Return New Response(ex)
         End Try
      End Function

      Private Function Post(Of TResponse)(uri As Uri, content As JSON.MultipartFormDataContent,
                                          headers As Dictionary(Of String, String),
                                          responseFactory As Func(Of String, String, Boolean, TResponse),
                                          succinct As Boolean) As Generic.Response(Of TResponse)
         Dim contentWriter As Action(Of IO.Stream) = If(content Is Nothing, Nothing,
                                                        New Action(Of IO.Stream)(Sub(requestStream As IO.Stream)
                                                                                    content.WriteTo(requestStream)
                                                                                 End Sub))
         Try
            Return New Generic.Response(Of TResponse)(GetResponse(uri, "POST", contentWriter, content.ContentType, headers, Nothing, Nothing),
                                                      Function(input As String, contentType As String) responseFactory(input, contentType, succinct))
         Catch ex As sn.WebException
            Return New Generic.Response(Of TResponse)(ex)
         End Try
      End Function
#End Region

#End Region

#Region "Deserialization of Responses"
      ''' <summary>
      ''' Deserializes a simple XmlSerializable
      ''' </summary>
      Private Function Deserialize(Of TResult As Serialization.XmlSerializable)(input As String, contentType As String, succinct As Boolean) As TResult
         If Not String.IsNullOrEmpty(contentType) AndAlso contentType.StartsWith(MediaTypes.Json, StringComparison.InvariantCultureIgnoreCase) Then
            Dim serializer As New JSON.Serialization.JavaScriptSerializer(succinct)
            Return serializer.Deserialize(Of TResult)(input)
         Else
            Return Nothing
         End If
      End Function

      ''' <summary>
      ''' Deserializes an array of XmlSerializable-instances
      ''' </summary>
      Private Function DeserializeArray(Of TResult As Serialization.XmlSerializable)(input As String, contentType As String, succinct As Boolean) As TResult()
         If Not String.IsNullOrEmpty(contentType) AndAlso contentType.StartsWith(MediaTypes.Json, StringComparison.InvariantCultureIgnoreCase) Then
            Dim serializer As New JSON.Serialization.JavaScriptSerializer(succinct)
            Return serializer.DeserializeArray(Of TResult)(input)
         Else
            Return Nothing
         End If
      End Function

      ''' <summary>
      ''' Deserializes cmisRepositoryInfoType-instances (transmitted as a map)
      ''' </summary>
      Private Function DeserializeRepositories(input As String, contentType As String, succinct As Boolean) As Core.cmisRepositoryInfoType()
         Try
            Dim serializer As New JSON.Serialization.JavaScriptSerializer(succinct)
            Return serializer.DeserializeMap(Of Core.cmisRepositoryInfoType, String)(input, Core.cmisRepositoryInfoType.DefaultKeyProperty)
         Catch ex As Exception
            Throw New System.ServiceModel.Web.WebFaultException(Of Exception)(ex, sn.HttpStatusCode.ExpectationFailed)
         End Try
      End Function
#End Region

      ''' <summary>
      ''' Copies object properties to content
      ''' </summary>
      Private Sub AddToContent(content As JSON.MultipartFormDataContent, properties As Core.Collections.cmisPropertiesType, policies As String(),
                               addACEs As Core.Security.cmisAccessControlListType, removeACEs As Core.Security.cmisAccessControlListType,
                               Optional contentStream As Messaging.cmisContentStreamType = Nothing,
                               Optional versioningState As Core.enumVersioningState? = Nothing)
         'properties
         If properties IsNot Nothing AndAlso properties.Properties IsNot Nothing Then
            For Each cmisProperty As Core.Properties.cmisProperty In properties.Properties
               content.Add(cmisProperty)
            Next
            If properties.Extensions IsNot Nothing Then
               content.Add(New CmisObjectModel.Core.cmisObjectType.PropertiesExtensions(properties))
            End If
         End If
         'policies
         If policies IsNot Nothing Then
            For Each policyId As String In policies
               content.Add(policyId, JSON.enumValueType.policy)
            Next
         End If
         'addACEs and removeACEs
         For Each de As KeyValuePair(Of JSON.enumCollectionAction, Core.Security.cmisAccessControlListType) In New Dictionary(Of JSON.enumCollectionAction, Core.Security.cmisAccessControlListType) From {
            {JSON.enumCollectionAction.add, addACEs}, {JSON.enumCollectionAction.remove, removeACEs}}
            If de.Value IsNot Nothing AndAlso de.Value.Permissions IsNot Nothing Then
               For Each ace As Core.Security.cmisAccessControlEntryType In de.Value.Permissions
                  content.Add(ace, de.Key)
               Next
            End If
         Next
         'contentStream (documents only)
         If contentStream IsNot Nothing Then
            Using ms As New IO.MemoryStream
               Dim stream As IO.Stream = contentStream.BinaryStream

               If stream IsNot Nothing Then
                  stream.CopyTo(ms)
                  ms.Position = 0
                  Dim httpContent As New JSON.HttpContent(ms.ToArray())
                  If Not String.IsNullOrEmpty(contentStream.Filename) Then
                     httpContent.Headers.Add(RFC2231Helper.ContentDispositionHeaderName,
                                             RFC2231Helper.EncodeContentDisposition(contentStream.Filename,
                                                                                    "form-data; name=""content"""))
                  End If
                  If Not String.IsNullOrEmpty(contentStream.MimeType) Then httpContent.Headers.Add(RFC2231Helper.ContentTypeHeaderName, contentStream.MimeType)
                  httpContent.Headers.Add(RFC2231Helper.ContentTransferEncoding, "binary")
                  content.Add(httpContent)
               End If
            End Using
         End If
         'versioningState (documents only)
         If versioningState.HasValue Then content.Add("versioningState", versioningState.Value.GetName())
      End Sub

      ''' <summary>
      ''' Returns the uri of an Object (ObjectByPath)
      ''' </summary>
      Private Function CreateObjectUri(rootFolderUri As String, relativePath As String) As String
         If String.IsNullOrEmpty(relativePath) Then
            Return rootFolderUri
         Else
            If Not String.IsNullOrEmpty(rootFolderUri) Then rootFolderUri = rootFolderUri.TrimEnd("/"c)
            relativePath = relativePath.TrimStart("/"c)
            Return rootFolderUri & "/" & relativePath
         End If
      End Function

      ''' <summary>
      ''' Returns the repository that matches the repositoryId-parameter or null, if no repository matches the filter
      ''' </summary>
      Private Function FindRepository(repositories As Core.cmisRepositoryInfoType(), repositoryId As String) As Core.cmisRepositoryInfoType
         If repositories IsNot Nothing Then
            For Each repository As Core.cmisRepositoryInfoType In repositories
               If repository IsNot Nothing AndAlso repository.RepositoryId = repositoryId Then Return repository
            Next
         End If

         'not found
         Return Nothing
      End Function

      ''' <summary>
      ''' Caches types
      ''' </summary>
      Private Shared Function SetTypeDefinitions(result As Messaging.cmisTypeDefinitionListType,
                                                 repositoryId As String) As Messaging.cmisTypeDefinitionListType
         If result IsNot Nothing AndAlso result.Types IsNot Nothing Then
            For Each type As Core.Definitions.Types.cmisTypeDefinitionType In result.Types
               If type IsNot Nothing Then
                  Dim id As String = type.Id
                  If Not String.IsNullOrEmpty(id) Then
                     TypeDefinition(repositoryId, id) = type
                  End If
               End If
            Next
         End If

         Return result
      End Function
      ''' <summary>
      ''' Caches types
      ''' </summary>
      Private Shared Function SetTypeDefinitions(result As Messaging.cmisTypeContainer(),
                                                 repositoryId As String) As Messaging.cmisTypeContainer()
         If result IsNot Nothing Then
            Dim containerCollections As New Stack(Of Messaging.cmisTypeContainer)(From container As Messaging.cmisTypeContainer In result
                                                                                  Where container IsNot Nothing
                                                                                  Select container)
            While containerCollections.Count > 0
               Dim container = containerCollections.Pop()
               Dim type As Core.Definitions.Types.cmisTypeDefinitionType = container.Type

               If type IsNot Nothing Then
                  Dim id As String = type.Id
                  If Not String.IsNullOrEmpty(id) Then
                     TypeDefinition(repositoryId, id) = type
                  End If
               End If
               If container.Children IsNot Nothing Then
                  For Each child As Messaging.cmisTypeContainer In container.Children
                     If child IsNot Nothing Then containerCollections.Push(child)
                  Next
               End If
            End While
         End If

         Return result
      End Function

      ''' <summary>
      ''' Cached TypeDefinitions to determine DateTime-properties
      ''' </summary>
      ''' <remarks></remarks>
      Private Shared _typeDefinitions As New Collections.Generic.DictionaryTree(Of String, Core.Definitions.Types.cmisTypeDefinitionType)
      Private Shared Property TypeDefinition(repositoryId As String, typeId As String) As Core.Definitions.Types.cmisTypeDefinitionType
         Get
            Return _typeDefinitions.Item(repositoryId, typeId)
         End Get
         Set(value As Core.Definitions.Types.cmisTypeDefinitionType)
            _typeDefinitions.Item(repositoryId, typeId) = value
         End Set
      End Property
   End Class
End Namespace