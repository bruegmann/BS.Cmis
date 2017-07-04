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
Imports cc = CmisObjectModel.Core
Imports ccc = CmisObjectModel.Core.Collections
Imports ccdt = CmisObjectModel.Core.Definitions.Types
Imports ccg = CmisObjectModel.Common.Generic
Imports cm = CmisObjectModel.Messaging
Imports cr = CmisObjectModel.RestAtom
Imports ss = System.ServiceModel
Imports ssw = System.ServiceModel.Web

'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.ServiceModel.Browser
   ''' <summary>
   ''' Implements the functionality of the cmis-webservice version 1.1
   ''' </summary>
   ''' <remarks>
   ''' The browser binding supports authentication with tokens for browser clients in the following way:
   ''' if the hosting system (that is the system which implements the ICmisServiceImpl) supports sessionIdCookie
   ''' this instance checks login processes. Three login methods are supported
   '''   a) via DispatchWebGetService() with query parameter repositoryId
   '''   b) via DispatchWebPostService() with query parameter cmisaction=login
   '''   c) via DispatchWebGetRepository() with query parameter cmisselector=repositoryInfo
   ''' In these three cases a check is made if the client wants the repository to enable authentication with tokens.
   ''' The client demands this sending a non empty token query parameter. If the client enables this authentication
   ''' type cross-site request forgery can be prevented.
   ''' </remarks>
   Public Class CmisService
      Inherits Base.CmisService
      Implements Contracts.IBrowserBinding

#Region "Constructors"
      Shared Sub New()
         If Constants.JSONTypeDefinitions.RepresentationTypes.ContainsKey(Constants.JSONTypeDefinitions.queryResultListUri) Then
            _queryResultListType = If(Constants.JSONTypeDefinitions.RepresentationTypes(Constants.JSONTypeDefinitions.queryResultListUri).RepresentationType,
                                      GetType(Messaging.cmisObjectListType))
         Else
            _queryResultListType = GetType(Messaging.cmisObjectListType)
         End If
      End Sub
#End Region

#Region "Helper classes"
      Private Enum enumTokenTransmission As Integer
         asControl
         asQueryParameter
      End Enum

      ''' <summary>
      ''' Contains a token sent from the client either as a form control or as a query parameter
      ''' </summary>
      ''' <remarks></remarks>
      Private Class Token

         Private Sub New(token As String, transmission As enumTokenTransmission)
            Me.Token = token
            Me.Transmission = transmission
         End Sub

         Public Shared Widening Operator CType(value As JSON.MultipartFormDataContent) As Token
            Dim token As String = If(value Is Nothing, Nothing, value.ToString("token"))

            Return If(String.IsNullOrEmpty(token), Nothing, New Token(token, enumTokenTransmission.asControl))
         End Operator

         Public Shared Widening Operator CType(value As String) As Token
            Return If(String.IsNullOrEmpty(value), Nothing, New Token(value, enumTokenTransmission.asQueryParameter))
         End Operator

         Public Shared Widening Operator CType(value As Token) As String
            Return If(value Is Nothing, Nothing, value.Token)
         End Operator

         ''' <summary>
         ''' Token; a non null value is guaranteed
         ''' </summary>
         ''' <remarks></remarks>
         Public ReadOnly Token As String
         Public ReadOnly Transmission As enumTokenTransmission

      End Class
#End Region

#Region "5.3.1. Service URL"
      ''' <summary>
      ''' Service-requests:
      ''' GetRepositories():                      defined in cmis
      ''' GetLoginPage(), GetEmbeddedFrame():     repository specific for browser binding
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function DispatchWebGetService() As System.IO.Stream Implements Contracts.IBrowserBinding.DispatchWebGetService
         Try
            CheckTokenAuthentication(Nothing)
            Select Case If(GetRequestParameter("file"), String.Empty).ToLowerInvariant()
               Case "loginpage.htm"
                  'GetLoginPage(); repository specific; see 5.2.9.2.2 Login and Tokens
                  Return GetFile(JSON.enumJSONFile.loginPageHtm)
               Case "embeddedframe.htm"
                  'GetEmbeddedFrame(); repository specific
                  Return GetFile(JSON.enumJSONFile.embeddedFrameHtm)
               Case "cmis.js"
                  Return GetFile(JSON.enumJSONFile.cmisJS)
               Case Else
                  Select Case If(GetRequestParameter("cmisaction"), String.Empty).ToLowerInvariant()
                     Case "login"
                        'GetLoginPage(); repository specific; see 5.2.9.2.2 Login and Tokens
                        Return GetFile(JSON.enumJSONFile.loginPageHtm)
                     Case Else
                        Return GetRepositories()
                  End Select
            End Select
         Catch ex As Exception
            Return SerializeException(ex)
         End Try
      End Function

      ''' <summary>
      ''' Service-post-requests
      ''' Login(), Logout():                    repository specific for browser binding
      ''' </summary>
      ''' <param name="stream"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function DispatchWebPostService(stream As IO.Stream) As IO.Stream Implements Contracts.IBrowserBinding.DispatchWebPostService
         Dim multipart As JSON.MultipartFormDataContent = JSON.MultipartFormDataContent.FromStream(stream, ssw.WebOperationContext.Current.IncomingRequest.ContentType)
         Dim cmisaction As String = If(multipart.ToString("cmisaction"), GetRequestParameter("cmisaction"))
         Dim repositoryId As String = If(multipart.ToString("repositoryId"), GetRequestParameter("repositoryId"))
         Dim serviceImpl = CmisServiceImpl
         Dim result As ccg.Result(Of Net.HttpStatusCode)

         Select Case cmisaction
            Case "login"
               'try to login
               Dim authorization As String = If(multipart.ToString("authorization"), GetRequestParameter("authorization"))
               Dim hostingApplicationUri As String = If(multipart.ToString("hostingApplicationUri"), GetRequestParameter("hostingApplicationUri"))
               Dim user As String = If(multipart.ToString("user"), GetRequestParameter("user"))

               result = serviceImpl.Login(repositoryId, authorization)
               If result.Failure IsNot Nothing OrElse result.Success <> Net.HttpStatusCode.OK Then
                  'failure => present the login page once more
                  Dim regEx As New System.Text.RegularExpressions.Regex("window\.history\.go\(\-2\)",
                                                                        Text.RegularExpressions.RegexOptions.Multiline Or Text.RegularExpressions.RegexOptions.IgnoreCase)
                  Return PrepareResult(regEx.Replace(My.Resources.RedirectPreviousPage, "window.history.back()"), Constants.MediaTypes.Html)
               Else
                  'sending a token signals enabling authentication with tokens
                  EnableTokenAuthentication()
                  'success => browse to window before login page
                  Return PrepareResult(My.Resources.RedirectPreviousPage, Constants.MediaTypes.Html)
               End If
            Case "logout"
               result = serviceImpl.Logout(repositoryId)
               System.ServiceModel.Web.WebOperationContext.Current.OutgoingResponse.StatusCode =
                  If(result.Failure Is Nothing, result.Success, Net.HttpStatusCode.InternalServerError)
            Case "ping"
               result = serviceImpl.Ping(repositoryId)
               System.ServiceModel.Web.WebOperationContext.Current.OutgoingResponse.StatusCode =
                  If(result.Failure Is Nothing, result.Success, Net.HttpStatusCode.InternalServerError)
         End Select

         'default response
         Return PrepareResult(My.Resources.EmptyPage, Constants.MediaTypes.Html)
      End Function
#End Region

#Region "5.3.2 Repository URL"
      Public Function DispatchWebGetRepository(repositoryId As String) As System.IO.Stream Implements Contracts.IBrowserBinding.DispatchWebGetRepository
         Try
            Dim serviceImpl As Contracts.ICmisServicesImpl = CmisServiceImpl
            Dim cmisselector As String = If(GetRequestParameter("cmisselector"), String.Empty)

            If String.IsNullOrEmpty(cmisselector) Then cmisselector = "repositoryInfo"
            If String.Compare(cmisselector, "lastResult", True) = 0 Then
               'GetLastResult() is the only request without a CheckTokenAuthentication()-call
               Return GetLastResult(serviceImpl, repositoryId)
            Else
               CheckTokenAuthentication(Nothing)
               Select Case cmisselector.ToLowerInvariant()
                  Case "checkedout"
                     Return GetCheckedOutDocs(serviceImpl, repositoryId, Nothing, ParseBoolean(GetRequestParameter("succinct")))
                  Case "contentchanges"
                     Return GetContentChanges(serviceImpl, repositoryId)
                  Case "query"
                     Return Query(serviceImpl, repositoryId)
                  Case "repositoryinfo"
                     Return GetRepositoryInfo(repositoryId)
                  Case "typechildren"
                     Return GetTypeChildren(serviceImpl, repositoryId)
                  Case "typedefinition"
                     Return GetTypeDefinition(serviceImpl, repositoryId)
                  Case "typedescendants"
                     Return GetTypeDescendants(serviceImpl, repositoryId)
                  Case Else
                     Throw LogException(Messaging.cmisFaultType.CreateInvalidArgumentException("The parameter 'cmisselector' is not valid.", False), serviceImpl)
               End Select
            End If
         Catch exWeb As System.ServiceModel.Web.WebFaultException
            If exWeb.StatusCode = Net.HttpStatusCode.Unauthorized Then
               Return GetFile(JSON.enumJSONFile.loginRefPageHtm)
            Else
               Return SerializeException(exWeb)
            End If
         Catch ex As Exception
            Return SerializeException(ex)
         End Try
      End Function

      Public Function DispatchWebPostRepository(repositoryId As String, stream As System.IO.Stream) As System.IO.Stream Implements Contracts.IBrowserBinding.DispatchWebPostRepository
         Dim token As Token = Nothing

         Try
            Dim multipart As JSON.MultipartFormDataContent = JSON.MultipartFormDataContent.FromStream(stream, ssw.WebOperationContext.Current.IncomingRequest.ContentType)
            Dim serviceImpl As Contracts.ICmisServicesImpl = CmisServiceImpl
            Dim cmisaction As String = If(multipart.ToString("cmisaction"), String.Empty)

            token = CheckTokenAuthentication(multipart)
            Select Case cmisaction.ToLowerInvariant()
               Case "bulkupdate"
                  Return BulkUpdateProperties(serviceImpl, repositoryId, token, multipart)
               Case "createdocument"
                  Return CreateDocument(serviceImpl, repositoryId, Nothing, token, multipart)
               Case "createdocumentfromsource"
                  Return CreateDocumentFromSource(serviceImpl, repositoryId, Nothing, token, multipart)
               Case "createitem"
                  Return CreateItem(serviceImpl, repositoryId, Nothing, token, multipart)
               Case "createpolicy"
                  Return CreatePolicy(serviceImpl, repositoryId, Nothing, token, multipart)
               Case "createrelationship"
                  Return CreateRelationship(serviceImpl, repositoryId, token, multipart)
               Case "createtype"
                  Return CreateType(serviceImpl, repositoryId, token, multipart)
               Case "deletetype"
                  Return DeleteType(serviceImpl, repositoryId, token, multipart)
               Case "query"
                  Return Query(serviceImpl, repositoryId, token, multipart)
               Case "updatetype"
                  Return UpdateType(serviceImpl, repositoryId, token, multipart)
               Case Else
                  Throw LogException(Messaging.cmisFaultType.CreateInvalidArgumentException("The parameter 'cmisaction' is not valid.", False), serviceImpl)
            End Select
         Catch ex As Exception
            Return SerializeException(ex, repositoryId, token)
         End Try
      End Function
#End Region

#Region "5.3.3 Root Folder URL (ObjectById)"
      Public Function DispatchWebGetRootFolder(repositoryId As String, objectId As String) As System.IO.Stream Implements Contracts.IBrowserBinding.DispatchWebGetRootFolder
         Try
            Dim serviceImpl As Contracts.ICmisServicesImpl = CmisServiceImpl
            Dim cmisselector As String = GetRequestParameter("cmisselector")
            Dim succinct As Boolean? = ParseBoolean(GetRequestParameter("succinct"))

            CheckTokenAuthentication(Nothing)
            'select default selector, if selector is not specified (chapter 5.4  Services)
            If String.IsNullOrEmpty(cmisselector) Then
               Select Case serviceImpl.GetBaseObjectType(repositoryId, objectId)
                  Case Core.enumBaseObjectTypeIds.cmisDocument
                     cmisselector = "content"
                  Case Core.enumBaseObjectTypeIds.cmisFolder
                     cmisselector = "children"
                  Case Else
                     cmisselector = "object"
               End Select
            End If
            Select Case cmisselector.ToLowerInvariant()
               Case "acl"
                  Return GetACL(serviceImpl, repositoryId, objectId)
               Case "allowableactions"
                  Return GetAllowableActions(serviceImpl, repositoryId, objectId, succinct)
               Case "checkedout"
                  Return GetCheckedOutDocs(serviceImpl, repositoryId, GetFolderId(serviceImpl, repositoryId, objectId), succinct)
               Case "children"
                  Return GetChildren(serviceImpl, repositoryId, objectId, succinct)
               Case "content"
                  Return GetContentStream(serviceImpl, repositoryId, objectId, succinct)
               Case "descendants"
                  Return GetDescendants(serviceImpl, repositoryId, objectId, succinct)
               Case "foldertree"
                  Return GetFolderTree(serviceImpl, repositoryId, objectId, succinct)
               Case "object"
                  Return GetObject(serviceImpl, repositoryId, objectId, succinct)
               Case "parent"
                  Return GetFolderParent(serviceImpl, repositoryId, objectId, succinct)
               Case "parents"
                  Return GetObjectParents(serviceImpl, repositoryId, objectId, succinct)
               Case "policies"
                  Return GetAppliedPolicies(serviceImpl, repositoryId, objectId, succinct)
               Case "properties"
                  Return GetProperties(serviceImpl, repositoryId, objectId, succinct)
               Case "relationships"
                  Return GetObjectRelationships(serviceImpl, repositoryId, objectId, succinct)
               Case "renditions"
                  Return GetRenditions(serviceImpl, repositoryId, objectId, succinct)
               Case "versions"
                  Return GetAllVersions(serviceImpl, repositoryId, objectId, succinct)
               Case Else
                  Throw LogException(Messaging.cmisFaultType.CreateInvalidArgumentException("The parameter 'cmisaction' is not valid.", False), serviceImpl)
            End Select
         Catch ex As Exception
            Return SerializeException(ex)
         End Try
      End Function

      Public Function DispatchWebPostRootFolder(repositoryId As String, objectId As String, stream As System.IO.Stream) As System.IO.Stream Implements Contracts.IBrowserBinding.DispatchWebPostRootFolder
         Dim token As Token = Nothing

         Try
            Dim multipart As JSON.MultipartFormDataContent = JSON.MultipartFormDataContent.FromStream(stream, ssw.WebOperationContext.Current.IncomingRequest.ContentType)
            Dim serviceImpl As Contracts.ICmisServicesImpl = CmisServiceImpl
            Dim cmisaction As String = If(multipart.ToString("cmisaction"), String.Empty)
            Dim succinct As Boolean? = ParseBoolean(multipart.ToString("succinct"))

            token = CheckTokenAuthentication(multipart)
            If objectId Is Nothing Then objectId = GetRequestParameter("id")
            Select Case cmisaction.ToLowerInvariant()
               Case "addobjecttofolder"
                  Return AddObjectToFolder(serviceImpl, repositoryId, objectId, token, multipart)
               Case "appendcontent"
                  Return AppendContentStream(serviceImpl, repositoryId, objectId, token, multipart)
               Case "applyacl"
                  Return ApplyACL(serviceImpl, repositoryId, objectId, token, multipart)
               Case "applypolicy"
                  Return ApplyPolicy(serviceImpl, repositoryId, objectId, token, multipart)
               Case "cancelcheckout"
                  Return CancelCheckOut(serviceImpl, repositoryId, objectId, token, multipart)
               Case "checkin"
                  Return CheckIn(serviceImpl, repositoryId, objectId, token, multipart)
               Case "checkout"
                  Return CheckOut(serviceImpl, repositoryId, objectId, token, multipart)
               Case "createdocument"
                  Return CreateDocument(CmisServiceImpl, repositoryId, GetFolderId(serviceImpl, repositoryId, objectId), token, multipart)
               Case "createdocumentfromsource"
                  Return CreateDocumentFromSource(serviceImpl, repositoryId, GetFolderId(serviceImpl, repositoryId, objectId), token, multipart)
               Case "createfolder"
                  Return CreateFolder(serviceImpl, repositoryId, GetFolderId(serviceImpl, repositoryId, objectId), token, multipart)
               Case "createitem"
                  Return CreateItem(serviceImpl, repositoryId, GetFolderId(serviceImpl, repositoryId, objectId), token, multipart)
               Case "createpolicy"
                  Return CreatePolicy(serviceImpl, repositoryId, GetFolderId(serviceImpl, repositoryId, objectId), token, multipart)
               Case "delete"
                  Return DeleteObject(serviceImpl, repositoryId, objectId, token, multipart)
               Case "deletecontent"
                  Return DeleteContentStream(serviceImpl, repositoryId, objectId, token, multipart)
               Case "deletetree"
                  Return DeleteTree(serviceImpl, repositoryId, GetFolderId(serviceImpl, repositoryId, objectId), token, multipart)
               Case "move"
                  Return MoveObject(serviceImpl, repositoryId, objectId, token, multipart)
               Case "removeobjectfromfolder"
                  Return RemoveObjectFromFolder(serviceImpl, repositoryId, objectId, token, multipart)
               Case "removepolicy"
                  Return RemovePolicy(serviceImpl, repositoryId, objectId, token, multipart)
               Case "setcontent"
                  Return SetContentStream(serviceImpl, repositoryId, objectId, token, multipart)
               Case "update"
                  Return UpdateProperties(serviceImpl, repositoryId, objectId, token, multipart)
               Case Else
                  Throw LogException(Messaging.cmisFaultType.CreateInvalidArgumentException("The parameter 'cmisaction' is not valid.", False), serviceImpl)
            End Select
         Catch ex As Exception
            Return SerializeException(ex, repositoryId, token)
         End Try
      End Function
#End Region

#Region "5.3.4 Object URL (ObjectByPath)"
      Public Function DispatchWebGetObjects(repositoryId As String, path As String) As IO.Stream Implements Contracts.IBrowserBinding.DispatchWebGetObjects
         Return DispatchWebGetRootFolder(repositoryId, If(CmisServiceImpl.GetObjectId(repositoryId, "/" & path), String.Empty))
      End Function

      Public Function DispatchWebPostObjects(repositoryId As String, path As String, stream As IO.Stream) As IO.Stream Implements Contracts.IBrowserBinding.DispatchWebPostObjects
         Return DispatchWebPostRootFolder(repositoryId, If(CmisServiceImpl.GetObjectId(repositoryId, path), String.Empty), stream)
      End Function
#End Region

#Region "Repository"
      ''' <summary>
      ''' Creates a new type
      ''' </summary>
      Private Function CreateType(serviceImpl As Contracts.ICmisServicesImpl,
                                  repositoryId As String,
                                  token As Token,
                                  data As JSON.MultipartFormDataContent) As IO.MemoryStream
         Dim result As ccg.Result(Of ccdt.cmisTypeDefinitionType)
         Dim newType As ccdt.cmisTypeDefinitionType = data.GetTypeDefinition()

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If newType Is Nothing Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("type"), serviceImpl)

         result = serviceImpl.CreateType(repositoryId, newType)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Return GetFormResponse(result.Success, repositoryId, token, Net.HttpStatusCode.Created)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Deletes a type definition
      ''' </summary>
      Private Function DeleteType(serviceImpl As Contracts.ICmisServicesImpl,
                                  repositoryId As String,
                                  token As Token,
                                  data As JSON.MultipartFormDataContent) As IO.MemoryStream
         Dim failure As Exception
         Dim typeId As String = data.ToString("typeId")

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(typeId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("typeId"), serviceImpl)

         failure = serviceImpl.DeleteType(repositoryId, typeId)
         If failure Is Nothing Then
            ssw.WebOperationContext.Current.OutgoingResponse.StatusCode = Net.HttpStatusCode.OK
            If IsHtmlPageRequired(token) Then
               Dim transaction As New JSON.Transaction() With {.Code = System.Net.HttpStatusCode.OK, .Exception = Nothing, .Message = Nothing, .ObjectId = typeId}

               Me.Transaction(repositoryId, token) = transaction
               Return PrepareResult(My.Resources.EmptyPage, MediaTypes.Html)
            Else
               Return Nothing
            End If
         End If

         'failure
         Throw LogException(failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Gets the transaction instance of the POST request identified by token
      ''' </summary>
      Private Function GetLastResult(serviceImpl As Contracts.ICmisServicesImpl,
                                     repositoryId As String) As IO.MemoryStream
         Dim token As String = If(GetRequestParameter("token"), String.Empty)
         Dim transaction As JSON.Transaction = Me.Transaction(token)

         If transaction Is Nothing Then
            'create fault transaction (see chapter 5.4.4.4  Access to Form Response Content)
            transaction = New JSON.Transaction() With {.Code = 0, .Exception = "invalidArgument", .Message = "The parameter 'token' is not valid.", .ObjectId = Nothing}
         Else
            'remove cookie
            Me.Transaction(repositoryId, token) = Nothing
         End If

         Return SerializeXmlSerializable(transaction)
      End Function

      ''' <summary>
      ''' Returns all available repositories
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Shadows Function GetRepositories() As IO.MemoryStream
         Try
            Return MyBase.GetRepositories(AddressOf SerializeRepositories)
         Finally
            'sending a token signals enabling authentication with tokens
            EnableTokenAuthentication()
         End Try
      End Function

      ''' <summary>
      ''' Returns the specified repository
      ''' </summary>
      Private Shadows Function GetRepositoryInfo(repositoryId As String) As IO.MemoryStream
         Try
            Return MyBase.GetRepositoryInfo(repositoryId, AddressOf SerializeRepositories)
         Finally
            'sending a token signals enabling authentication with tokens
            EnableTokenAuthentication()
         End Try
      End Function

      ''' <summary>
      ''' Returns all child types of the specified type, if defined, otherwise the basetypes of the repository.
      ''' </summary>
      Private Function GetTypeChildren(serviceImpl As Contracts.ICmisServicesImpl,
                                       repositoryId As String) As IO.MemoryStream
         Dim result As ccg.Result(Of cm.cmisTypeDefinitionListType)
         'get the optional parameters from the queryString
         Dim typeId As String = If(GetRequestParameter(ServiceURIs.enumTypesUri.typeId), GetRequestParameter("id"))
         Dim includePropertyDefinitions As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumTypesUri.includePropertyDefinitions))
         Dim maxItems As xs_Integer? = ParseInteger(GetRequestParameter(ServiceURIs.enumTypesUri.maxItems))
         Dim skipCount As xs_Integer? = ParseInteger(GetRequestParameter(ServiceURIs.enumTypesUri.skipCount))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)

         result = serviceImpl.GetTypeChildren(repositoryId, typeId,
                                              includePropertyDefinitions.HasValue AndAlso includePropertyDefinitions.Value,
                                              maxItems, skipCount)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Return SerializeXmlSerializable(result.Success)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Returns the type-definition of the specified type
      ''' </summary>
      Private Function GetTypeDefinition(serviceImpl As Contracts.ICmisServicesImpl,
                                         repositoryId As String) As IO.MemoryStream
         Dim result As ccg.Result(Of ccdt.cmisTypeDefinitionType)
         Dim typeId As String = If(GetRequestParameter(ServiceURIs.enumTypesUri.typeId), GetRequestParameter("id"))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(typeId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("typeId"), serviceImpl)
         result = serviceImpl.GetTypeDefinition(repositoryId, typeId)

         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Return SerializeXmlSerializable(result.Success)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Returns the descendant object-types under the specified type.
      ''' </summary>
      Private Function GetTypeDescendants(serviceImpl As Contracts.ICmisServicesImpl,
                                          repositoryId As String) As IO.MemoryStream
         Dim result As ccg.Result(Of cm.cmisTypeContainer)
         Dim typeId As String = If(GetRequestParameter(ServiceURIs.enumTypeDescendantsUri.typeId), GetRequestParameter("id"))
         Dim depth As xs_Integer? = ParseInteger(GetRequestParameter(ServiceURIs.enumTypeDescendantsUri.depth))
         Dim includePropertyDefinitions As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumTypeDescendantsUri.includePropertyDefinitions))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If depth.HasValue AndAlso (depth.Value = 0 OrElse depth.Value < -1) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("The parameter 'depth' MUST NOT be 0 or less than -1", False), serviceImpl)

         result = serviceImpl.GetTypeDescendants(repositoryId, typeId, includePropertyDefinitions.HasValue AndAlso includePropertyDefinitions.Value, depth)

         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Dim typeContainer As cm.cmisTypeContainer = result.Success

            If typeContainer.Type Is Nothing Then
               'no typeId defined
               Return SerializeArray(typeContainer.Children)
            Else
               'typeId defined
               Return SerializeArray(New cm.cmisTypeContainer() {typeContainer})
            End If
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Updates a type definition
      ''' </summary>
      Private Function UpdateType(serviceImpl As Contracts.ICmisServicesImpl,
                                  repositoryId As String,
                                  token As Token,
                                  data As JSON.MultipartFormDataContent) As IO.MemoryStream
         Dim result As ccg.Result(Of ccdt.cmisTypeDefinitionType)
         Dim type As ccdt.cmisTypeDefinitionType = data.GetTypeDefinition()

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If type Is Nothing Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("type"), serviceImpl)

         result = serviceImpl.UpdateType(repositoryId, type)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Return GetFormResponse(result.Success, repositoryId, token, Net.HttpStatusCode.OK)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function
#End Region

#Region "Navigation"
      ''' <summary>
      ''' Returns a list of check out object the user has access to.
      ''' </summary>
      Private Function GetCheckedOutDocs(serviceImpl As Contracts.ICmisServicesImpl,
                                         repositoryId As String,
                                         folderId As String,
                                         succinct As Boolean?) As IO.MemoryStream
         Dim result As ccg.Result(Of cmisObjectListType)
         'get the optional parameters from the queryString
         Dim maxItems As xs_Integer? = ParseInteger(GetRequestParameter(ServiceURIs.enumCheckedOutUri.maxItems))
         Dim skipCount As xs_Integer? = ParseInteger(GetRequestParameter(ServiceURIs.enumCheckedOutUri.skipCount))
         Dim orderBy As String = GetRequestParameter(ServiceURIs.enumCheckedOutUri.orderBy)
         Dim filter As String = GetRequestParameter(ServiceURIs.enumCheckedOutUri.filter)
         Dim includeAllowableActions As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumCheckedOutUri.includeAllowableActions))
         Dim includeRelationships As Core.enumIncludeRelationships? = ParseEnum(Of Core.enumIncludeRelationships)(GetRequestParameter(ServiceURIs.enumCheckedOutUri.includeRelationships))
         Dim renditionFilter As String = GetRequestParameter(ServiceURIs.enumCheckedOutUri.renditionFilter)

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)

         result = serviceImpl.GetCheckedOutDocs(repositoryId, folderId, filter, maxItems, skipCount,
                                                renditionFilter, includeAllowableActions, includeRelationships)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Return SerializeXmlSerializable(New Messaging.Responses.getContentChangesResponse() With {.Objects = result.Success},
                                            succinct:=succinct)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Returns all children of the specified CMIS object.
      ''' </summary>
      Private Function GetChildren(serviceImpl As Contracts.ICmisServicesImpl,
                                   repositoryId As String,
                                   folderId As String,
                                   succinct As Boolean?) As IO.MemoryStream
         Dim result As ccg.Result(Of cmisObjectInFolderListType)
         'get optional parameters from the queryString
         Dim maxItems As xs_Integer? = ParseInteger(GetRequestParameter(ServiceURIs.enumChildrenUri.maxItems))
         Dim skipCount As xs_Integer? = ParseInteger(GetRequestParameter(ServiceURIs.enumChildrenUri.skipCount))
         Dim filter As String = GetRequestParameter(ServiceURIs.enumChildrenUri.filter)
         Dim includeAllowableActions As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumChildrenUri.includeAllowableActions))
         Dim includeRelationships As Core.enumIncludeRelationships? = ParseEnum(Of Core.enumIncludeRelationships)(GetRequestParameter(ServiceURIs.enumChildrenUri.includeRelationships))
         Dim renditionFilter As String = GetRequestParameter(ServiceURIs.enumChildrenUri.renditionFilter)
         Dim orderBy As String = GetRequestParameter(ServiceURIs.enumChildrenUri.orderBy)
         Dim includePathSegment As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumChildrenUri.includePathSegment))

         If String.IsNullOrEmpty(folderId) Then folderId = If(GetRequestParameter(ServiceURIs.enumChildrenUri.folderId), GetRequestParameter("id"))
         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(folderId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("folderId"), serviceImpl)

         result = serviceImpl.GetChildren(repositoryId, folderId, maxItems, skipCount, filter,
                                          includeAllowableActions, includeRelationships,
                                          renditionFilter, orderBy,
                                          includePathSegment.HasValue AndAlso includePathSegment.Value)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Return SerializeXmlSerializable(Of Messaging.cmisObjectInFolderListType)(result.Success, succinct)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Returns the descendant objects contained in the specified folder or any of its child-folders.
      ''' </summary>
      Private Function GetDescendants(serviceImpl As Contracts.ICmisServicesImpl,
                                      repositoryId As String,
                                      folderId As String,
                                      succinct As Boolean?) As IO.MemoryStream
         Dim result As ccg.Result(Of cmisObjectInFolderContainerType)
         Dim filter As String = GetRequestParameter("filter")
         Dim depth As xs_Integer? = ParseInteger(GetRequestParameter("depth"))
         Dim includeAllowableActions As Boolean? = ParseBoolean(GetRequestParameter("includeAllowableActions"))
         Dim includePathSegment As Boolean? = ParseBoolean(GetRequestParameter("includePathSegment"))
         Dim includeRelationships As Core.enumIncludeRelationships? = ParseEnum(Of Core.enumIncludeRelationships)(GetRequestParameter("includeRelationships"))
         Dim renditionFilter As String = GetRequestParameter("renditionFilter")

         If String.IsNullOrEmpty(folderId) Then folderId = If(GetRequestParameter(ServiceURIs.enumChildrenUri.folderId), GetRequestParameter("id"))
         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(folderId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("folderId"), serviceImpl)
         If depth.HasValue AndAlso (depth.Value = 0 OrElse depth.Value < -1) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("The parameter 'depth' MUST NOT be 0 or less than -1", False), serviceImpl)

         result = serviceImpl.GetDescendants(repositoryId, folderId, filter, depth,
                                             includeAllowableActions, includeRelationships,
                                             renditionFilter, includePathSegment.HasValue AndAlso includePathSegment.Value)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Return SerializeArray(If(result.Success, New cmisObjectInFolderContainerType()).Children, succinct)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Returns the parent folder-object of the specified folder
      ''' </summary>
      Private Function GetFolderParent(serviceImpl As Contracts.ICmisServicesImpl,
                                       repositoryId As String,
                                       folderId As String,
                                       succinct As Boolean?) As IO.MemoryStream
         Dim result As ccg.Result(Of cmisObjectType)
         Dim filter As String = GetRequestParameter("filter")

         If String.IsNullOrEmpty(folderId) Then folderId = If(GetRequestParameter(ServiceURIs.enumChildrenUri.folderId), GetRequestParameter("id"))
         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(folderId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("folderId"), serviceImpl)

         result = serviceImpl.GetFolderParent(repositoryId, folderId, filter)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Return SerializeXmlSerializable(Of Core.cmisObjectType)(result.Success, succinct)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Returns the descendant folders contained in the specified folder
      ''' </summary>
      Private Function GetFolderTree(serviceImpl As Contracts.ICmisServicesImpl,
                                     repositoryId As String,
                                     folderId As String,
                                     succinct As Boolean?) As IO.MemoryStream
         Dim result As ccg.Result(Of cmisObjectInFolderContainerType)
         Dim filter As String = GetRequestParameter(ServiceURIs.enumFolderTreeUri.filter)
         Dim depth As xs_Integer? = ParseInteger(GetRequestParameter(ServiceURIs.enumFolderTreeUri.depth))
         Dim includeAllowableActions As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumFolderTreeUri.includeAllowableActions))
         Dim includeRelationships As Core.enumIncludeRelationships? = ParseEnum(Of Core.enumIncludeRelationships)(GetRequestParameter(ServiceURIs.enumFolderTreeUri.includeRelationships))
         Dim includePathSegment As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumFolderTreeUri.includePathSegment))
         Dim renditionFilter As String = GetRequestParameter(ServiceURIs.enumFolderTreeUri.renditionFilter)

         If String.IsNullOrEmpty(folderId) Then folderId = If(GetRequestParameter(ServiceURIs.enumChildrenUri.folderId), GetRequestParameter("id"))
         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(folderId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("folderId"), serviceImpl)
         If depth.HasValue AndAlso (depth.Value = 0 OrElse depth.Value < -1) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("depth"), serviceImpl)

         result = serviceImpl.GetFolderTree(repositoryId, folderId, filter, depth,
                                            includeAllowableActions, includeRelationships,
                                            includePathSegment.HasValue AndAlso includePathSegment.Value,
                                            renditionFilter)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Return SerializeArray(Of Messaging.cmisObjectInFolderContainerType)(If(result.Success, New cmisObjectInFolderContainerType()).Children, succinct)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Returns the parent folders for the specified object
      ''' </summary>
      Private Function GetObjectParents(serviceImpl As Contracts.ICmisServicesImpl,
                                        repositoryId As String,
                                        objectId As String,
                                        succinct As Boolean?) As IO.MemoryStream
         Dim result As ccg.Result(Of cmisObjectParentsType())
         Dim filter As String = GetRequestParameter("filter")
         Dim includeAllowableActions As Boolean? = ParseBoolean(GetRequestParameter("includeAllowableActions"))
         Dim includeRelativePathSegment As Boolean? = ParseBoolean(GetRequestParameter("includeRelativePathSegment"))
         Dim includeRelationships As Core.enumIncludeRelationships? = ParseEnum(Of Core.enumIncludeRelationships)(GetRequestParameter("includeRelationships"))
         Dim renditionFilter As String = GetRequestParameter("renditionFilter")

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

         result = CmisServiceImpl.GetObjectParents(repositoryId, objectId, filter, includeAllowableActions,
                                                   includeRelationships, renditionFilter, includeRelativePathSegment)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Dim parents As Messaging.cmisObjectParentsType() = If(result.Success Is Nothing, Nothing,
                                                                  (From parent As Messaging.cmisObjectParentsType In result.Success
                                                                   Select parent).ToArray())
            Return SerializeArray(parents, succinct)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function
#End Region

#Region "Object"
      ''' <summary>
      ''' Appends to the content stream for the specified document object.
      ''' </summary>
      Private Function AppendContentStream(serviceImpl As Contracts.ICmisServicesImpl,
                                           repositoryId As String,
                                           objectId As String,
                                           token As Token,
                                           data As JSON.MultipartFormDataContent) As IO.MemoryStream
         Dim result As ccg.Result(Of cm.Responses.setContentStreamResponse)
         Dim isLastChunk As Boolean? = ParseBoolean(data.ToString("isLastChunk"))
         Dim changeToken As String = data.ToString("changeToken")
         Dim httpContent As JSON.HttpContent = data.Content("content")
         Dim content As Byte() = If(httpContent Is Nothing, Nothing, httpContent.Value)
         Dim succinct As Boolean? = ParseBoolean(data.ToString("succinct"))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)
         If content Is Nothing Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("content"), serviceImpl)

         result = serviceImpl.AppendContentStream(repositoryId, objectId, New IO.MemoryStream(content) With {.Position = 0},
                                                  httpContent.Headers.Item(RFC2231Helper.ContentTypeHeaderName),
                                                  RFC2231Helper.DecodeContentDisposition(httpContent.Headers(RFC2231Helper.ContentDispositionHeaderName), ""),
                                                  isLastChunk.HasValue AndAlso isLastChunk.Value, changeToken)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            If IsHtmlPageRequired(token) Then
               Dim transaction As New JSON.Transaction() With {.Code = System.Net.HttpStatusCode.OK, .Exception = Nothing, .Message = Nothing, .ObjectId = result.Success.ObjectId}

               Me.Transaction(repositoryId, token) = transaction
               Return PrepareResult(My.Resources.EmptyPage, MediaTypes.Html)
            Else
               Return GetObject(serviceImpl, repositoryId, result.Success.ObjectId, succinct)
            End If
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Updates properties and secondary types of one or more objects
      ''' </summary>
      Private Function BulkUpdateProperties(serviceImpl As Contracts.ICmisServicesImpl,
                                            repositoryId As String,
                                            token As Token,
                                            data As JSON.MultipartFormDataContent) As IO.MemoryStream
         Dim result As ccg.Result(Of cmisObjectListType)
         Dim changeTokens As String() = data.GetAutoIndexedValues(JSON.enumValueType.changeToken)
         Dim objectIds As String() = data.GetAutoIndexedValues(JSON.enumValueType.objectId)
         Dim length As Integer = If(changeTokens Is Nothing, 0, changeTokens.Length)

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If changeTokens Is Nothing OrElse objectIds Is Nothing OrElse length = 0 OrElse objectIds.Length <> length Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectIdAndChangeToken"), serviceImpl)
         End If

         Dim bulkUpdate As New Core.cmisBulkUpdateType() With {
            .AddSecondaryTypeIds = data.GetSecondaryTypeIds(JSON.enumCollectionAction.add),
            .Properties = data.GetProperties(Function(typeId) serviceImpl.TypeDefinition(repositoryId, typeId)),
            .RemoveSecondaryTypeIds = data.GetSecondaryTypeIds(JSON.enumCollectionAction.remove)}
         Dim objectIdAndChangeTokens As New List(Of cc.cmisObjectIdAndChangeTokenType)(length)

         For index As Integer = 0 To length - 1
            objectIdAndChangeTokens.Add(New cc.cmisObjectIdAndChangeTokenType() With {.ChangeToken = changeTokens(index), .Id = objectIds(index)})
         Next
         bulkUpdate.ObjectIdAndChangeTokens = objectIdAndChangeTokens.ToArray()

         result = serviceImpl.BulkUpdateProperties(repositoryId, bulkUpdate)

         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            'GetFormResponse() cannot be used because bulkUpdateProperties returns an array of cmisObjectIdAndChangeTokenType, not a single cmisObject
            If IsHtmlPageRequired(token) Then
               Dim transaction As New JSON.Transaction() With {.Code = System.Net.HttpStatusCode.OK, .Exception = Nothing, .Message = Nothing, .ObjectId = Nothing}

               Me.Transaction(repositoryId, token) = transaction
               Return PrepareResult(My.Resources.EmptyPage, MediaTypes.Html)
            Else
               Dim list = If(result.Success, New cmisObjectListType())
               Dim objects = If(list.Objects, New cmisObjectType() {})

               Return SerializeArray((From obj As cmisObjectType In objects
                                      Select New Core.cmisObjectIdAndChangeTokenType() With {.ChangeToken = obj.ChangeToken,
                                                                                             .Id = obj.ObjectId,
                                                                                             .NewId = obj.BulkUpdateProperties.NewId}).ToArray())
            End If
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Creates a new document in the specified folder or as unfiled document
      ''' </summary>
      Private Function CreateDocument(serviceImpl As Contracts.ICmisServicesImpl,
                                      repositoryId As String,
                                      folderId As String,
                                      token As Token,
                                      data As JSON.MultipartFormDataContent) As IO.MemoryStream
         Dim result As ccg.Result(Of cmisObjectType)
         Dim hasProperties As Boolean = False
         Dim repositoryInfo As Core.cmisRepositoryInfoType = serviceImpl.RepositoryInfo(repositoryId)
         Dim httpContent As JSON.HttpContent = data.Content("content")
         Dim content As Messaging.cmisContentStreamType = If(httpContent Is Nothing, Nothing,
                                                             New Messaging.cmisContentStreamType(New IO.MemoryStream(If(httpContent.Value, New Byte() {})) With {.Position = 0},
                                                                                                 RFC2231Helper.DecodeContentDisposition(httpContent.Headers(RFC2231Helper.ContentDispositionHeaderName), ""),
                                                                                                 httpContent.Headers.Item(RFC2231Helper.ContentTypeHeaderName)))
         Dim policyIds As String() = data.GetAutoIndexedValues(JSON.enumValueType.policy)
         Dim properties As ccc.cmisPropertiesType = data.GetProperties(Function(typeId)
                                                                          'at least the cmis:objectTypeId MUST be set
                                                                          hasProperties = True
                                                                          Return serviceImpl.TypeDefinition(repositoryId, typeId)
                                                                       End Function)
         Dim versioningState As Core.enumVersioningState? = ParseEnum(Of Core.enumVersioningState)(data.ToString("versioningState"))
         Dim succinct As Boolean? = ParseBoolean(data.ToString("succinct"))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) OrElse repositoryInfo Is Nothing Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         End If
         If String.IsNullOrEmpty(folderId) AndAlso Not repositoryInfo.Capabilities.CapabilityUnfiling Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("folderId"), serviceImpl)
         End If
         If Not hasProperties Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("properties"), serviceImpl)
         End If

         result = serviceImpl.CreateDocument(repositoryId, New cc.cmisObjectType() With {.Properties = properties, .PolicyIds = New ccc.cmisListOfIdsType(policyIds)},
                                             folderId, content, versioningState,
                                             data.GetACEs(JSON.enumCollectionAction.add), data.GetACEs(JSON.enumCollectionAction.remove))
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Return GetFormResponse(result.Success, repositoryId, token, Net.HttpStatusCode.Created, succinct)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Creates a document object as a copy of the given source document in the (optionally) specified location
      ''' </summary>
      ''' <remarks>In chapter 5.4.2.7 Action "createDocumentFromSource" the listed relevant CMIS controls contains "Content",
      ''' but there is no equivalent in chapter 2.2.4.2  createDocumentFromSource. Therefore content is ignored.</remarks>
      Private Function CreateDocumentFromSource(serviceImpl As Contracts.ICmisServicesImpl,
                                                repositoryId As String,
                                                folderId As String,
                                                token As Token,
                                                data As JSON.MultipartFormDataContent) As IO.MemoryStream
         Dim result As ccg.Result(Of cmisObjectType)
         Dim repositoryInfo As Core.cmisRepositoryInfoType = serviceImpl.RepositoryInfo(repositoryId)
         Dim policyIds As String() = data.GetAutoIndexedValues(JSON.enumValueType.policy)
         Dim properties As ccc.cmisPropertiesType = data.GetProperties(Function(typeId) serviceImpl.TypeDefinition(repositoryId, typeId))
         Dim sourceId As String = data.ToString("sourceId")
         Dim succinct As Boolean? = ParseBoolean(data.ToString("succinct"))
         Dim versioningState As Core.enumVersioningState? = ParseEnum(Of Core.enumVersioningState)(data.ToString("versioningState"))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) OrElse repositoryInfo Is Nothing Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         End If
         If String.IsNullOrEmpty(sourceId) OrElse Not CmisServiceImpl.Exists(repositoryId, sourceId) Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("sourceId"), serviceImpl)
         End If
         If String.IsNullOrEmpty(folderId) AndAlso Not repositoryInfo.Capabilities.CapabilityUnfiling Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("folderId"), serviceImpl)
         End If

         result = serviceImpl.CreateDocumentFromSource(repositoryId, sourceId, properties, folderId, versioningState, policyIds,
                                                       data.GetACEs(JSON.enumCollectionAction.add), data.GetACEs(JSON.enumCollectionAction.remove))
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Return GetFormResponse(result.Success, repositoryId, token, Net.HttpStatusCode.Created, succinct)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Creates a folder object of the specified type in the specified location
      ''' </summary>
      Private Function CreateFolder(serviceImpl As Contracts.ICmisServicesImpl,
                                    repositoryId As String,
                                    parentFolderId As String,
                                    token As Token,
                                    data As JSON.MultipartFormDataContent) As IO.MemoryStream
         Dim result As ccg.Result(Of cmisObjectType)
         Dim hasProperties As Boolean = False
         Dim policyIds As String() = data.GetAutoIndexedValues(JSON.enumValueType.policy)
         Dim properties As ccc.cmisPropertiesType = data.GetProperties(Function(typeId)
                                                                          'at least the cmis:objectTypeId MUST be set
                                                                          hasProperties = True
                                                                          Return serviceImpl.TypeDefinition(repositoryId, typeId)
                                                                       End Function)
         Dim succinct As Boolean? = ParseBoolean(data.ToString("succinct"))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(parentFolderId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("folderId"), serviceImpl)
         If Not hasProperties Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("properties"), serviceImpl)
         End If

         result = serviceImpl.CreateFolder(repositoryId, New cc.cmisObjectType() With {.Properties = properties, .PolicyIds = New ccc.cmisListOfIdsType(policyIds)},
                                           parentFolderId, data.GetACEs(JSON.enumCollectionAction.add), data.GetACEs(JSON.enumCollectionAction.remove))
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Return GetFormResponse(result.Success, repositoryId, token, Net.HttpStatusCode.Created, succinct)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Creates an item object of the specified type in the specified location
      ''' </summary>
      Private Function CreateItem(serviceImpl As Contracts.ICmisServicesImpl,
                                  repositoryId As String,
                                  folderId As String,
                                  token As Token,
                                  data As JSON.MultipartFormDataContent) As IO.MemoryStream
         Dim result As ccg.Result(Of cmisObjectType)
         Dim hasProperties As Boolean = False
         Dim repositoryInfo As Core.cmisRepositoryInfoType = serviceImpl.RepositoryInfo(repositoryId)
         Dim policyIds As String() = data.GetAutoIndexedValues(JSON.enumValueType.policy)
         Dim properties As ccc.cmisPropertiesType = data.GetProperties(Function(typeId)
                                                                          'at least the cmis:objectTypeId MUST be set
                                                                          hasProperties = True
                                                                          Return serviceImpl.TypeDefinition(repositoryId, typeId)
                                                                       End Function)
         Dim succinct As Boolean? = ParseBoolean(data.ToString("succinct"))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) OrElse repositoryInfo Is Nothing Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         End If
         If String.IsNullOrEmpty(folderId) AndAlso Not repositoryInfo.Capabilities.CapabilityUnfiling Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("folderId"), serviceImpl)
         End If
         If Not hasProperties Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("properties"), serviceImpl)
         End If

         result = serviceImpl.CreateItem(repositoryId, New cc.cmisObjectType() With {.Properties = properties, .PolicyIds = New ccc.cmisListOfIdsType(policyIds)},
                                         folderId, data.GetACEs(JSON.enumCollectionAction.add), data.GetACEs(JSON.enumCollectionAction.remove))
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Return GetFormResponse(result.Success, repositoryId, token, Net.HttpStatusCode.Created, succinct)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Creates a policy object of the specified type in the specified location
      ''' </summary>
      Private Function CreatePolicy(serviceImpl As Contracts.ICmisServicesImpl,
                                    repositoryId As String,
                                    folderId As String,
                                    token As Token,
                                    data As JSON.MultipartFormDataContent) As IO.MemoryStream
         Dim result As ccg.Result(Of cmisObjectType)
         Dim hasProperties As Boolean = False
         Dim repositoryInfo As Core.cmisRepositoryInfoType = serviceImpl.RepositoryInfo(repositoryId)
         Dim policyIds As String() = data.GetAutoIndexedValues(JSON.enumValueType.policy)
         Dim properties As ccc.cmisPropertiesType = data.GetProperties(Function(typeId)
                                                                          'at least the cmis:objectTypeId MUST be set
                                                                          hasProperties = True
                                                                          Return serviceImpl.TypeDefinition(repositoryId, typeId)
                                                                       End Function)
         Dim succinct As Boolean? = ParseBoolean(data.ToString("succinct"))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) OrElse repositoryInfo Is Nothing Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         End If
         If String.IsNullOrEmpty(folderId) AndAlso Not repositoryInfo.Capabilities.CapabilityUnfiling Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("folderId"), serviceImpl)
         End If
         If Not hasProperties Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("properties"), serviceImpl)
         End If

         result = serviceImpl.CreatePolicy(repositoryId, New cc.cmisObjectType() With {.Properties = properties, .PolicyIds = New ccc.cmisListOfIdsType(policyIds)},
                                           folderId, data.GetACEs(JSON.enumCollectionAction.add), data.GetACEs(JSON.enumCollectionAction.remove))
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Return GetFormResponse(result.Success, repositoryId, token, Net.HttpStatusCode.Created, succinct)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Creates a relationship object of the specified type
      ''' </summary>
      Private Function CreateRelationship(serviceImpl As Contracts.ICmisServicesImpl,
                                          repositoryId As String,
                                          token As Token,
                                          data As JSON.MultipartFormDataContent) As IO.MemoryStream
         Dim result As ccg.Result(Of cmisObjectType)
         Dim hasProperties As Boolean = False
         Dim policyIds As String() = data.GetAutoIndexedValues(JSON.enumValueType.policy)
         Dim properties As ccc.cmisPropertiesType = data.GetProperties(Function(typeId)
                                                                          'at least the cmis:objectTypeId MUST be set
                                                                          hasProperties = True
                                                                          Return serviceImpl.TypeDefinition(repositoryId, typeId)
                                                                       End Function)
         Dim succinct As Boolean? = ParseBoolean(data.ToString("succinct"))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         End If
         If Not hasProperties Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("properties"), serviceImpl)
         End If

         result = serviceImpl.CreateRelationship(repositoryId, New cc.cmisObjectType() With {.Properties = properties, .PolicyIds = New ccc.cmisListOfIdsType(policyIds)},
                                                 data.GetACEs(JSON.enumCollectionAction.add), data.GetACEs(JSON.enumCollectionAction.remove))
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Return GetFormResponse(result.Success, repositoryId, token, Net.HttpStatusCode.Created, succinct)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Deletes the content stream of a cmis document
      ''' </summary>
      Private Function DeleteContentStream(serviceImpl As Contracts.ICmisServicesImpl,
                                           repositoryId As String,
                                           objectId As String,
                                           token As Token,
                                           data As JSON.MultipartFormDataContent) As IO.MemoryStream
         Dim result As ccg.Result(Of cm.Responses.deleteContentStreamResponse)
         Dim changeToken As String = data.ToString("changeToken")
         Dim succinct As Boolean? = ParseBoolean(data.ToString("succinct"))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

         result = serviceImpl.DeleteContentStream(repositoryId, objectId, changeToken)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            If IsHtmlPageRequired(token) Then
               Dim transaction As New JSON.Transaction() With {.Code = System.Net.HttpStatusCode.OK, .Exception = Nothing, .Message = Nothing, .ObjectId = result.Success.ObjectId}

               Me.Transaction(repositoryId, token) = transaction
               Return PrepareResult(My.Resources.EmptyPage, MediaTypes.Html)
            Else
               Return GetObject(serviceImpl, repositoryId, result.Success.ObjectId, succinct)
            End If
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Removes the submitted document
      ''' </summary>
      Private Function DeleteObject(serviceImpl As Contracts.ICmisServicesImpl,
                                    repositoryId As String,
                                    objectId As String,
                                    token As Token,
                                    data As JSON.MultipartFormDataContent) As IO.MemoryStream
         Dim failure As Exception
         Dim allVersions As Boolean? = ParseBoolean(data.ToString("allVersions"))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

         failure = serviceImpl.DeleteObject(repositoryId, objectId, Not allVersions.HasValue OrElse allVersions.Value)
         If failure Is Nothing Then
            ssw.WebOperationContext.Current.OutgoingResponse.StatusCode = Net.HttpStatusCode.OK
            If IsHtmlPageRequired(token) Then
               Dim transaction As New JSON.Transaction() With {.Code = System.Net.HttpStatusCode.OK, .Exception = Nothing, .Message = Nothing, .ObjectId = objectId}

               Me.Transaction(repositoryId, token) = transaction
               Return PrepareResult(My.Resources.EmptyPage, MediaTypes.Html)
            Else
               Return Nothing
            End If
         End If

         'failure
         Throw LogException(failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Deletes the specified folder object and all of its child- and descendant-objects.
      ''' </summary>
      Private Function DeleteTree(serviceImpl As Contracts.ICmisServicesImpl,
                                  repositoryId As String,
                                  folderId As String,
                                  token As Token,
                                  data As JSON.MultipartFormDataContent) As IO.MemoryStream
         Dim result As ccg.Result(Of cm.Responses.deleteTreeResponse)
         Dim allVersion As Boolean? = ParseBoolean(data.ToString("allVersions"))
         Dim unfileObjects As Core.enumUnfileObject? = ParseEnum(Of Core.enumUnfileObject)(data.ToString("unfileObjects"))
         Dim continueOnFailure As Boolean? = ParseBoolean(data.ToString("continueOnFailure"))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(folderId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("folderId"), serviceImpl)

         result = serviceImpl.DeleteTree(repositoryId, folderId, Not allVersion.HasValue OrElse allVersion.Value,
                                         unfileObjects, continueOnFailure.HasValue AndAlso continueOnFailure.Value)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Dim objectIds As String() = If(result.Success Is Nothing OrElse result.Success.FailedToDelete Is Nothing, Nothing, result.Success.FailedToDelete.ObjectIds)

            ssw.WebOperationContext.Current.OutgoingResponse.StatusCode = Net.HttpStatusCode.OK
            If objectIds Is Nothing OrElse objectIds.Length = 0 Then
               Return Nothing
            Else
               Return SerializeXmlSerializable(New Core.Collections.cmisListOfIdsType(objectIds))
            End If
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Returns the allowable actions for the specified document.
      ''' </summary>
      Private Function GetAllowableActions(serviceImpl As Contracts.ICmisServicesImpl,
                                           repositoryId As String,
                                           objectId As String,
                                           succinct As Boolean?) As IO.MemoryStream
         Dim result As ccg.Result(Of Core.cmisAllowableActionsType)

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

         result = serviceImpl.GetAllowableActions(repositoryId, objectId)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Return SerializeXmlSerializable(result.Success, succinct)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Returns the content stream of the specified object.
      ''' </summary>
      Private Function GetContentStream(serviceImpl As Contracts.ICmisServicesImpl,
                                        repositoryId As String,
                                        objectId As String,
                                        succinct As Boolean?) As IO.MemoryStream
         Dim result As ccg.Result(Of cm.cmisContentStreamType)
         Dim streamId As String = GetRequestParameter(ServiceURIs.enumContentUri.streamId)
         Dim download As String = GetRequestParameter("download")

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

         result = serviceImpl.GetContentStream(repositoryId, objectId, streamId)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Dim contentStream As cm.cmisContentStreamType = result.Success

            If contentStream Is Nothing Then
               ssw.WebOperationContext.Current.OutgoingResponse.StatusCode = Net.HttpStatusCode.OK
               Return Nothing
            Else
               Dim fileName As String = If(String.IsNullOrEmpty(contentStream.Filename), "NotSet", contentStream.Filename)
               Dim binaryStream As IO.Stream = contentStream.BinaryStream

               download = If(String.Compare("attachment", download, True) = 0, "attachment", "inline")
               ssw.WebOperationContext.Current.OutgoingResponse.StatusCode = contentStream.StatusCode
               ssw.WebOperationContext.Current.OutgoingResponse.ContentType = contentStream.MimeType
               ssw.WebOperationContext.Current.OutgoingResponse.Headers.Add(RFC2231Helper.ContentDispositionHeaderName, RFC2231Helper.EncodeContentDisposition(fileName, download))
               If TypeOf binaryStream Is IO.MemoryStream Then
                  binaryStream.Position = 0
                  Return CType(binaryStream, IO.MemoryStream)
               Else
                  Dim retVal As New IO.MemoryStream

                  contentStream.BinaryStream.CopyTo(retVal)
                  retVal.Position = 0
                  Return retVal
               End If
            End If
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Returns the cmisobject with the specified id.
      ''' </summary>
      ''' <remarks>Method supports privateWorkingCopy-parameter, not defined in chapter 5.4.3.13 Selector "object".
      ''' Implements the services GetObject(), GetObjectByPath() and GetObjectOfLatest().</remarks>
      Private Function GetObject(serviceImpl As Contracts.ICmisServicesImpl,
                                 repositoryId As String,
                                 objectId As String,
                                 succinct As Boolean?) As IO.MemoryStream
         Dim result As ccg.Result(Of cmisObjectType)
         'optional parameters from the queryString
         Dim filter As String = GetRequestParameter(ServiceURIs.enumObjectUri.filter)
         Dim includeRelationships As Core.enumIncludeRelationships? = ParseEnum(Of Core.enumIncludeRelationships)(GetRequestParameter(ServiceURIs.enumObjectUri.includeRelationships))
         Dim includePolicyIds As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumObjectUri.includePolicyIds))
         Dim renditionFilter As String = GetRequestParameter(ServiceURIs.enumObjectUri.renditionFilter)
         Dim includeACL As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumObjectUri.includeACL))
         Dim includeAllowableActions As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumObjectUri.includeAllowableActions))
         Dim returnVersion As RestAtom.enumReturnVersion? = ParseEnum(Of RestAtom.enumReturnVersion)(GetRequestParameter(ServiceURIs.enumObjectUri.returnVersion))
         Dim major As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumObjectUri.major))
         Dim privateWorkingCopy As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumObjectUri.pwc))

         'getObjectOfLatestVersion: parameter versionSeriesId is used instead of objectId and parameter major instead of returnVersion
         If String.IsNullOrEmpty(objectId) Then
            objectId = GetRequestParameter(ServiceURIs.enumObjectUri.versionSeriesId)
            If Not String.IsNullOrEmpty(objectId) Then
               returnVersion = If(major.HasValue AndAlso major.Value, RestAtom.enumReturnVersion.latestmajor, RestAtom.enumReturnVersion.latest)
            End If
         ElseIf Not returnVersion.HasValue AndAlso major.HasValue Then
            returnVersion = If(major.Value, RestAtom.enumReturnVersion.latestmajor, RestAtom.enumReturnVersion.latest)
         End If
         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

         result = serviceImpl.GetObject(repositoryId, objectId, filter, includeRelationships, includePolicyIds, renditionFilter, includeACL, includeAllowableActions, returnVersion, privateWorkingCopy)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Return SerializeXmlSerializable(Of Core.cmisObjectType)(result.Success, succinct)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function
      ''' <summary>
      ''' Returns the object at the specified path
      ''' </summary>
      ''' <remarks>Using the browser binding every service-call that targets an object can be made using the objectId queryString-parameter
      ''' or the path of that object. That means not only the GetObject() method has a pendant using the path of the object, but all services
      ''' addressing an object can be called using the objectId-parameter or the path of the object. So a special GetObject()/GetObjectByPath()-
      ''' couple is obsolete in the browser binding.</remarks>
      <Obsolete("Use GetObject instead.", True)>
      Private Function GetObjectByPath(serviceImpl As Contracts.ICmisServicesImpl,
                                       repositoryId As String,
                                       objectId As String,
                                       succinct As Boolean?) As IO.MemoryStream
         Return GetObject(serviceImpl, repositoryId, objectId, succinct)
      End Function

      ''' <summary>
      ''' Get the properties of an object.
      ''' </summary>
      ''' <remarks>Implements the services GetProperties() and GetPropertiesOfLatestVersion().</remarks>
      Private Function GetProperties(serviceImpl As Contracts.ICmisServicesImpl,
                                     repositoryId As String,
                                     objectId As String,
                                     succinct As Boolean?) As IO.MemoryStream
         Dim result As ccg.Result(Of cmisObjectType)
         Dim filter As String = GetRequestParameter(ServiceURIs.enumObjectUri.filter)
         Dim returnVersion As RestAtom.enumReturnVersion? = ParseEnum(Of RestAtom.enumReturnVersion)(GetRequestParameter(ServiceURIs.enumObjectUri.returnVersion))
         Dim major As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumObjectUri.major))

         'getPropertiesOfLatestVersion: parameter versionSeriesId is used instead of objectId and parameter major instead of returnVersion
         If String.IsNullOrEmpty(objectId) Then
            objectId = GetRequestParameter(ServiceURIs.enumObjectUri.versionSeriesId)
            If Not String.IsNullOrEmpty(objectId) Then
               returnVersion = If(major.HasValue AndAlso major.Value, RestAtom.enumReturnVersion.latestmajor, RestAtom.enumReturnVersion.latest)
            End If
         ElseIf Not returnVersion.HasValue AndAlso major.HasValue Then
            returnVersion = If(major.Value, RestAtom.enumReturnVersion.latestmajor, RestAtom.enumReturnVersion.latest)
         End If
         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

         result = serviceImpl.GetObject(repositoryId, objectId, filter, Core.enumIncludeRelationships.none, False, "cmis:none", False, False, returnVersion, Nothing)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Dim properties As Core.Collections.cmisPropertiesType = If(result.Success Is Nothing, Nothing, result.Success.Properties)
            Return SerializeXmlSerializable(properties, succinct)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Get the properties of an object.
      ''' </summary>
      Private Function GetRenditions(serviceImpl As Contracts.ICmisServicesImpl,
                                     repositoryId As String,
                                     objectId As String,
                                     succinct As Boolean?) As IO.MemoryStream
         Dim result As ccg.Result(Of cmisObjectType)
         'optional parameters from the queryString
         Dim renditionFilter As String = GetRequestParameter(ServiceURIs.enumObjectUri.renditionFilter)
         Dim maxItems As xs_Integer? = ParseInteger(GetRequestParameter("maxItems"))
         Dim skipCount As xs_Integer? = ParseInteger(GetRequestParameter("skipCount"))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

         result = serviceImpl.GetObject(repositoryId, objectId, CmisPredefinedPropertyNames.ObjectId, Core.enumIncludeRelationships.none, False, renditionFilter, False, False, Nothing, Nothing)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Dim renditions As Core.cmisRenditionType() = If(result.Success Is Nothing, Nothing, result.Success.Renditions)

            If renditions IsNot Nothing AndAlso (maxItems.HasValue OrElse skipCount.HasValue) Then
               Dim nMaxItems As xs_Integer = If(maxItems.HasValue, maxItems.Value, xs_Integer.MaxValue)
               Dim nSkipCount As xs_Integer = If(skipCount.HasValue, skipCount.Value, 0)
               Dim length As xs_Integer

#If xs_Integer = "Int32" OrElse xs_Integer = "Integer" OrElse xs_Integer = "Single" Then
               length = renditions.Length
#Else
               length = renditions.LongLength
#End If
               If nSkipCount >= length Then
                  renditions = New Core.cmisRenditionType() {}
               Else
                  Dim newLength As xs_Integer = Math.Min(length - nSkipCount, nMaxItems)
                  Dim cutout As Core.cmisRenditionType() = CType(Array.CreateInstance(GetType(Core.cmisRenditionType), newLength), Core.cmisRenditionType())
                  Array.Copy(renditions, nSkipCount, cutout, 0, newLength)
                  renditions = cutout
               End If
            End If
            Return SerializeArray(renditions, succinct)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Moves the specified file-able object from one folder to another
      ''' </summary>
      Private Function MoveObject(serviceImpl As Contracts.ICmisServicesImpl,
                                  repositoryId As String,
                                  objectId As String,
                                  token As Token,
                                  data As JSON.MultipartFormDataContent) As IO.MemoryStream
         Dim result As ccg.Result(Of cmisObjectType)
         Dim targetFolderId As String = data.ToString("targetFolderId")
         Dim sourceFolderId As String = data.ToString("sourceFolderId")
         Dim succinct As Boolean? = ParseBoolean(data.ToString("succinct"))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)
         If String.IsNullOrEmpty(targetFolderId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("targetFolderId"), serviceImpl)
         If String.IsNullOrEmpty(sourceFolderId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("sourceFolderId"), serviceImpl)

         result = serviceImpl.MoveObject(repositoryId, objectId, targetFolderId, sourceFolderId)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Return GetFormResponse(result.Success, repositoryId, token, Net.HttpStatusCode.Created, succinct)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Sets the content stream of the specified object.
      ''' </summary>
      Private Function SetContentStream(serviceImpl As Contracts.ICmisServicesImpl,
                                        repositoryId As String,
                                        objectId As String,
                                        token As Token,
                                        data As JSON.MultipartFormDataContent) As IO.MemoryStream
         Dim result As ccg.Result(Of cm.Responses.setContentStreamResponse)
         Dim overwriteFlag As Boolean? = ParseBoolean(data.ToString("overwriteFlag"))
         Dim changeToken As String = data.ToString("changeToken")
         Dim httpContent As JSON.HttpContent = data.Content("content")
         Dim content As Byte() = If(httpContent Is Nothing, Nothing, httpContent.Value)
         Dim succinct As Boolean? = ParseBoolean(data.ToString("succinct"))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)
         If content Is Nothing Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("content"), serviceImpl)

         result = serviceImpl.SetContentStream(repositoryId, objectId, New IO.MemoryStream(content) With {.Position = 0},
                                               httpContent.Headers.Item(RFC2231Helper.ContentTypeHeaderName),
                                               RFC2231Helper.DecodeContentDisposition(httpContent.Headers(RFC2231Helper.ContentDispositionHeaderName), ""),
                                               Not overwriteFlag.HasValue OrElse overwriteFlag.Value, changeToken)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            If IsHtmlPageRequired(token) Then
               Dim transaction As New JSON.Transaction() With {.Code = System.Net.HttpStatusCode.OK, .Exception = Nothing, .Message = Nothing, .ObjectId = result.Success.ObjectId}

               Me.Transaction(repositoryId, token) = transaction
               Return PrepareResult(My.Resources.EmptyPage, MediaTypes.Html)
            Else
               Return GetObject(serviceImpl, repositoryId, result.Success.ObjectId, succinct)
            End If
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Updates the submitted cmis-object
      ''' </summary>
      Private Function UpdateProperties(serviceImpl As Contracts.ICmisServicesImpl,
                                        repositoryId As String,
                                        objectId As String,
                                        token As Token,
                                        data As JSON.MultipartFormDataContent) As IO.MemoryStream
         Dim result As ccg.Result(Of cmisObjectType)
         Dim changeToken As String = data.ToString("changeToken")
         Dim typeDefinitions As Core.Definitions.Types.cmisTypeDefinitionType()
         Dim properties As ccc.cmisPropertiesType
         Dim succinct As Boolean? = ParseBoolean(data.ToString("succinct"))

         typeDefinitions = GetTypeDefinitions(serviceImpl, repositoryId, objectId)
         properties = data.GetProperties(Function(typeId)
                                            Return serviceImpl.TypeDefinition(repositoryId, typeId)
                                         End Function, typeDefinitions)
         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)
         If properties Is Nothing OrElse properties.Count = 0 Then
            Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("properties"), serviceImpl)
         End If

         result = serviceImpl.UpdateProperties(repositoryId, objectId, properties, changeToken)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Return GetFormResponse(result.Success, repositoryId, token, If(result.Success Is Nothing OrElse result.Success.ObjectId <> objectId,
                                                                           Net.HttpStatusCode.Created, Net.HttpStatusCode.OK), succinct)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function
#End Region

#Region "Multi"
      ''' <summary>
      ''' Adds an existing fileable non-folder object to a folder.
      ''' </summary>
      Private Function AddObjectToFolder(serviceImpl As Contracts.ICmisServicesImpl,
                                         repositoryId As String,
                                         objectId As String,
                                         token As Token,
                                         data As JSON.MultipartFormDataContent) As IO.MemoryStream
         Dim result As ccg.Result(Of cmisObjectType)
         Dim folderId As String = data.ToString("folderId")
         Dim allVersions As Boolean? = ParseBoolean(If(data.ToString("allVersions"), "true"))
         Dim succinct As Boolean? = ParseBoolean(data.ToString("succinct"))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)
         If String.IsNullOrEmpty(folderId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("folderId"), serviceImpl)

         result = serviceImpl.AddObjectToFolder(repositoryId, objectId, folderId, Not allVersions.HasValue OrElse allVersions.Value)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Return GetFormResponse(result.Success, repositoryId, token, Net.HttpStatusCode.Created, succinct)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Removes an existing fileable non-folder object from a folder.
      ''' </summary>
      Private Function RemoveObjectFromFolder(serviceImpl As Contracts.ICmisServicesImpl,
                                              repositoryId As String,
                                              objectId As String,
                                              token As Token,
                                              data As JSON.MultipartFormDataContent) As IO.MemoryStream
         Dim result As ccg.Result(Of cmisObjectType)
         Dim folderId As String = data.ToString("folderId")
         Dim succinct As Boolean? = ParseBoolean(data.ToString("succinct"))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

         result = serviceImpl.RemoveObjectFromFolder(repositoryId, objectId, folderId)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Return GetFormResponse(result.Success, repositoryId, token, Net.HttpStatusCode.Created, succinct)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function
#End Region

#Region "Discovery"
      ''' <summary>
      ''' Returns a list of content changes
      ''' </summary>
      ''' <remarks>Filter parameter is not specified in the cmis documentation for browser binding, but included in the corresponding
      ''' definition in the atompub binding. Therefore the browser binding supports the filter parameter as well.</remarks>
      Private Function GetContentChanges(serviceImpl As Contracts.ICmisServicesImpl,
                                         repositoryId As String) As IO.MemoryStream
         Dim result As ccg.Result(Of getContentChanges)
         Dim changeLogToken As String = GetRequestParameter("changeLogToken")
         Dim filter As String = GetRequestParameter("filter")
         Dim maxItems As xs_Integer? = ParseInteger(GetRequestParameter("maxItems"))
         Dim includeACL As Boolean? = ParseBoolean(GetRequestParameter("includeACL"))
         Dim includePolicyIds As Boolean? = ParseBoolean(GetRequestParameter("includePolicyIds"))
         Dim includeProperties As Boolean? = ParseBoolean(GetRequestParameter("includeProperties"))
         Dim succinct As Boolean? = ParseBoolean(GetRequestParameter("succinct"))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)

         result = serviceImpl.GetContentChanges(repositoryId, filter, maxItems, includeACL,
                                                includePolicyIds.HasValue AndAlso includePolicyIds.Value,
                                                includeProperties.HasValue AndAlso includeProperties.Value,
                                                changeLogToken)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Return SerializeXmlSerializable(Of Messaging.Responses.getContentChangesResponse)(result.Success, succinct)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Returns the data described by the specified CMIS query. (GET Request)
      ''' </summary>
      Private Function Query(serviceImpl As Contracts.ICmisServicesImpl,
                             repositoryId As String) As IO.MemoryStream
         Return Query(serviceImpl, repositoryId, Nothing, ParseBoolean(GetRequestParameter("succinct")),
                      If(GetRequestParameter("q"), GetRequestParameter("statement")), ParseBoolean(GetRequestParameter("searchAllVersions")),
                      ParseEnum(Of Core.enumIncludeRelationships)(GetRequestParameter("includeRelationships")),
                      GetRequestParameter("renditionFilter"), ParseBoolean(GetRequestParameter("includeAllowableActions")),
                      ParseInteger(GetRequestParameter("maxItems")), ParseInteger(GetRequestParameter("skipCount")))
      End Function

      ''' <summary>
      ''' Returns the data described by the specified CMIS query. (POST Request)
      ''' </summary>
      Private Function Query(serviceImpl As Contracts.ICmisServicesImpl,
                             repositoryId As String,
                             token As Token,
                             data As JSON.MultipartFormDataContent) As IO.MemoryStream
         Return Query(serviceImpl, repositoryId, token, ParseBoolean(data.ToString("succinct")),
                      If(data.ToString("statement"), data.ToString("q")), ParseBoolean(data.ToString("searchAllVersions")),
                      ParseEnum(Of Core.enumIncludeRelationships)(data.ToString("includeRelationships")),
                      data.ToString("renditionFilter"), ParseBoolean(data.ToString("includeAllowableActions")),
                      ParseInteger(data.ToString("maxItems")), ParseInteger(data.ToString("skipCount")))
      End Function
      Private Function Query(serviceImpl As Contracts.ICmisServicesImpl,
                             repositoryId As String, token As Token, succinct As Boolean?,
                             q As String, searchAllVersions As Boolean?, includeRelationships As Core.enumIncludeRelationships?,
                             renditionFilter As String, includeAllowableActions As Boolean?, maxItems As xs_Integer?, skipCount As xs_Integer?) As IO.MemoryStream
         Dim result As ccg.Result(Of cmisObjectListType)

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(q) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("statement"), serviceImpl)
         result = serviceImpl.Query(repositoryId, q, searchAllVersions.HasValue AndAlso searchAllVersions.Value, includeRelationships,
                                                renditionFilter, includeAllowableActions.HasValue AndAlso includeAllowableActions.Value,
                                                maxItems, skipCount)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            'GetFormResponse() cannot be used because bulkUpdateProperties returns an array of cmisObjectIdAndChangeTokenType, not a single cmisObject
            If IsHtmlPageRequired(token) Then
               Dim transaction As New JSON.Transaction() With {.Code = System.Net.HttpStatusCode.OK, .Exception = Nothing, .Message = Nothing, .ObjectId = Nothing}

               Me.Transaction(repositoryId, token) = transaction
               Return PrepareResult(My.Resources.EmptyPage, MediaTypes.Html)
            Else
               Return SerializeXmlSerializable(Of Messaging.cmisObjectListType)(result.Success, succinct)
            End If
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function
#End Region

#Region "Versioning"
      ''' <summary>
      ''' Reverses the effect of a check-out (checkOut). Removes the Private Working Copy of the checked-out document, allowing other documents in the version series to be checked out again.
      ''' If the private working copy has been created by createDocument, cancelCheckOut MUST delete the created document.
      ''' </summary>
      Private Function CancelCheckOut(serviceImpl As Contracts.ICmisServicesImpl,
                                      repositoryId As String,
                                      objectId As String,
                                      token As Token,
                                      data As JSON.MultipartFormDataContent) As IO.MemoryStream
         Dim failure As Exception

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

         failure = serviceImpl.CancelCheckOut(repositoryId, objectId)
         If failure Is Nothing Then
            ssw.WebOperationContext.Current.OutgoingResponse.StatusCode = Net.HttpStatusCode.OK
            Return Nothing
         ElseIf Not IsWebException(failure) Then
            failure = cm.cmisFaultType.CreateUnknownException(failure)
         End If

         'failure
         Throw LogException(failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Checks-in the Private Working Copy document.
      ''' </summary>
      Private Function CheckIn(serviceImpl As Contracts.ICmisServicesImpl,
                               repositoryId As String,
                               objectId As String,
                               token As Token,
                               data As JSON.MultipartFormDataContent) As IO.MemoryStream
         Dim result As ccg.Result(Of cmisObjectType)
         Dim properties As ccc.cmisPropertiesType
         Dim typeDefinitions As Core.Definitions.Types.cmisTypeDefinitionType()
         Dim major As Boolean? = ParseBoolean(data.ToString("major"))
         Dim httpContent As JSON.HttpContent = data.Content("content")
         Dim content As Messaging.cmisContentStreamType = If(httpContent Is Nothing, Nothing,
                                                             New Messaging.cmisContentStreamType(New IO.MemoryStream(If(httpContent.Value, New Byte() {})) With {.Position = 0},
                                                                                                 RFC2231Helper.DecodeContentDisposition(httpContent.Headers(RFC2231Helper.ContentDispositionHeaderName), ""),
                                                                                                 httpContent.Headers.Item(RFC2231Helper.ContentTypeHeaderName)))
         Dim checkInComment As String = data.ToString("checkInComment")
         Dim policyIds As String() = data.GetAutoIndexedValues(JSON.enumValueType.policy)
         Dim addACEs As Core.Security.cmisAccessControlListType = data.GetACEs(JSON.enumCollectionAction.add)
         Dim removeACEs As Core.Security.cmisAccessControlListType = data.GetACEs(JSON.enumCollectionAction.remove)
         Dim succinct As Boolean? = ParseBoolean(data.ToString("succinct"))

         typeDefinitions = GetTypeDefinitions(serviceImpl, repositoryId, objectId)
         properties = data.GetProperties(Function(typeId)
                                            Return serviceImpl.TypeDefinition(repositoryId, typeId)
                                         End Function, typeDefinitions)
         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

         result = serviceImpl.CheckIn(repositoryId, objectId, properties, policyIds, content,
                                      Not major.HasValue OrElse major.Value, checkInComment, addACEs, removeACEs)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Return GetFormResponse(result.Success, repositoryId, token, Net.HttpStatusCode.Created, succinct)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Checks out the specified CMIS object.
      ''' </summary>
      Private Function CheckOut(serviceImpl As Contracts.ICmisServicesImpl,
                                repositoryId As String,
                                objectId As String,
                                token As Token,
                                data As JSON.MultipartFormDataContent) As IO.MemoryStream
         Dim result As ccg.Result(Of cmisObjectType)
         Dim succinct As Boolean? = ParseBoolean(data.ToString("succinct"))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

         result = serviceImpl.CheckOut(repositoryId, objectId)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Return GetFormResponse(result.Success, repositoryId, token, Net.HttpStatusCode.Created, succinct)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Returns all Documents in the specified version series.
      ''' </summary>
      ''' <param name="serviceImpl"></param>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="succinct"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function GetAllVersions(serviceImpl As Contracts.ICmisServicesImpl,
                                      repositoryId As String,
                                      objectId As String,
                                      succinct As Boolean?) As IO.MemoryStream
         Dim result As ccg.Result(Of cmisObjectListType)
         Dim versionSeriesId As String = GetRequestParameter(ServiceURIs.enumAllVersionsUri.versionSeriesId)
         Dim filter As String = GetRequestParameter(ServiceURIs.enumAllVersionsUri.filter)
         Dim includeAllowableActions As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumAllVersionsUri.includeAllowableActions))

         If String.IsNullOrEmpty(objectId) Then objectId = GetRequestParameter(ServiceURIs.enumAllVersionsUri.versionSeriesId)
         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) AndAlso String.IsNullOrEmpty(versionSeriesId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

         result = serviceImpl.GetAllVersions(repositoryId, objectId, versionSeriesId, filter, includeAllowableActions)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Dim objects As cmisObjectType() = If(result.Success Is Nothing, Nothing, result.Success.Objects)
            Return SerializeArray(objects, succinct)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Get the latest document object in the version series
      ''' </summary>
      <Obsolete("Use GetObject instead.", True)>
      Private Function GetObjectOfLatestVersion(serviceImpl As Contracts.ICmisServicesImpl,
                                                repositoryId As String,
                                                objectId As String,
                                                succinct As Boolean?) As IO.MemoryStream
         Return GetObject(serviceImpl, repositoryId, objectId, succinct)
      End Function

      ''' <summary>
      ''' Get a subset of the properties for the latest document object in the version series.
      ''' </summary>
      <Obsolete("Use GetProperties instead.", True)>
      Private Function GetPropertiesOfLatestVersion(serviceImpl As Contracts.ICmisServicesImpl,
                                                    repositoryId As String,
                                                    objectId As String,
                                                    succinct As Boolean?) As IO.MemoryStream
         Return GetProperties(serviceImpl, repositoryId, objectId, succinct)
      End Function
#End Region

#Region "Relationships"
      ''' <summary>
      ''' Returns the relationships for the specified object.
      ''' </summary>
      Private Function GetObjectRelationships(serviceImpl As Contracts.ICmisServicesImpl,
                                              repositoryId As String,
                                              objectId As String,
                                              succinct As Boolean?) As IO.MemoryStream
         Dim result As ccg.Result(Of cmisObjectListType)
         Dim includeSubRelationshipTypes As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumRelationshipsUri.includeSubRelationshipTypes))
         Dim relationshipDirection As Core.enumRelationshipDirection? = ParseEnum(Of Core.enumRelationshipDirection)(GetRequestParameter(ServiceURIs.enumRelationshipsUri.relationshipDirection))
         Dim typeId As String = GetRequestParameter(ServiceURIs.enumRelationshipsUri.typeId)
         Dim maxItems As xs_Integer? = ParseInteger(GetRequestParameter(ServiceURIs.enumRelationshipsUri.maxItems))
         Dim skipCount As xs_Integer? = ParseInteger(GetRequestParameter(ServiceURIs.enumRelationshipsUri.skipCount))
         Dim filter As String = GetRequestParameter(ServiceURIs.enumRelationshipsUri.filter)
         Dim includeAllowableActions As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumRelationshipsUri.includeAllowableActions))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

         result = serviceImpl.GetObjectRelationships(repositoryId, objectId,
                                                     includeSubRelationshipTypes.HasValue AndAlso includeSubRelationshipTypes.Value,
                                                     relationshipDirection, typeId, maxItems, skipCount, filter, includeAllowableActions)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Return SerializeXmlSerializable(New Messaging.Responses.getContentChangesResponse() With {.Objects = result.Success}, succinct)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function
#End Region

#Region "Policy"
      ''' <summary>
      ''' Applies a policy to the specified object.
      ''' </summary>
      Private Function ApplyPolicy(serviceImpl As Contracts.ICmisServicesImpl,
                                   repositoryId As String,
                                   objectId As String,
                                   token As Token,
                                   data As JSON.MultipartFormDataContent) As IO.MemoryStream
         Dim result As ccg.Result(Of cmisObjectType)
         Dim policyId As String = data.ToString("policyId")
         Dim succinct As Boolean? = ParseBoolean(data.ToString("succinct"))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)
         If String.IsNullOrEmpty(policyId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("policyId"), serviceImpl)

         result = serviceImpl.ApplyPolicy(repositoryId, objectId, policyId)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Return GetFormResponse(result.Success, repositoryId, token, Net.HttpStatusCode.OK, succinct)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Returns a list of policies applied to the specified object.
      ''' </summary>
      Private Function GetAppliedPolicies(serviceImpl As Contracts.ICmisServicesImpl,
                                          repositoryId As String,
                                          objectId As String,
                                          succinct As Boolean?) As IO.MemoryStream
         Dim result As ccg.Result(Of cmisObjectListType)
         Dim filter As String = GetRequestParameter(ServiceURIs.enumPoliciesUri.filter)

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

         result = serviceImpl.GetAppliedPolicies(repositoryId, objectId, filter)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Dim objects As cmisObjectType() = If(result.Success Is Nothing, Nothing, result.Success.Objects)
            Return SerializeArray(objects, succinct)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Removes a policy from the specified object.
      ''' </summary>
      Private Function RemovePolicy(serviceImpl As Contracts.ICmisServicesImpl,
                                    repositoryId As String,
                                    objectId As String,
                                    token As Token,
                                    data As JSON.MultipartFormDataContent) As IO.MemoryStream
         Dim failure As Exception
         Dim policyId As String = data.ToString("policyId")
         Dim succinct As Boolean? = ParseBoolean(data.ToString("succinct"))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)
         If String.IsNullOrEmpty(policyId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("policyId"), serviceImpl)

         failure = serviceImpl.RemovePolicy(repositoryId, objectId, policyId)
         If failure Is Nothing Then
            If IsHtmlPageRequired(token) Then
               Dim transaction As New JSON.Transaction() With {.Code = System.Net.HttpStatusCode.OK, .Exception = Nothing, .Message = Nothing, .ObjectId = objectId}

               Me.Transaction(repositoryId, token) = transaction
               Return PrepareResult(My.Resources.EmptyPage, MediaTypes.Html)
            Else
               Return GetObject(serviceImpl, repositoryId, objectId, succinct)
            End If
         Else
            'failure
            Throw LogException(failure, serviceImpl)
         End If
      End Function
#End Region

#Region "ACL"
      ''' <summary>
      ''' Adds or removes the given ACEs to or from the ACL of document or folder object.
      ''' </summary>
      Private Function ApplyACL(serviceImpl As Contracts.ICmisServicesImpl,
                                repositoryId As String,
                                objectId As String,
                                token As Token,
                                data As JSON.MultipartFormDataContent) As IO.MemoryStream
         Dim result As ccg.Result(Of Core.Security.cmisAccessControlListType)
         Dim aclPropagation As Core.enumACLPropagation? = data.GetACLPropagation
         Dim addACEs As Core.Security.cmisAccessControlListType = data.GetACEs(JSON.enumCollectionAction.add)
         Dim removeACEs As Core.Security.cmisAccessControlListType = data.GetACEs(JSON.enumCollectionAction.remove)

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

         result = serviceImpl.ApplyACL(repositoryId, objectId, addACEs, removeACEs,
                                       If(aclPropagation.HasValue, aclPropagation.Value,
                                          Core.enumACLPropagation.repositorydetermined))
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            If IsHtmlPageRequired(token) Then
               Dim transaction As New JSON.Transaction() With {.Code = System.Net.HttpStatusCode.OK, .Exception = Nothing, .Message = Nothing, .ObjectId = objectId}

               Me.Transaction(repositoryId, token) = transaction
               Return PrepareResult(My.Resources.EmptyPage, MediaTypes.Html)
            Else
               Return SerializeXmlSerializable(result.Success)
            End If
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function

      ''' <summary>
      ''' Get the ACL currently applied to the specified document or folder object.
      ''' </summary>
      Private Function GetACL(serviceImpl As Contracts.ICmisServicesImpl,
                              repositoryId As String,
                              objectId As String) As IO.MemoryStream
         Dim result As ccg.Result(Of Core.Security.cmisAccessControlListType)
         Dim onlyBasicPermissions As Boolean? = ParseBoolean(GetRequestParameter(ServiceURIs.enumACLUri.onlyBasicPermissions))

         'invalid arguments
         If String.IsNullOrEmpty(repositoryId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("repositoryId"), serviceImpl)
         If String.IsNullOrEmpty(objectId) Then Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("objectId"), serviceImpl)

         result = serviceImpl.GetACL(repositoryId, objectId, Not onlyBasicPermissions.HasValue OrElse onlyBasicPermissions.Value)
         If result Is Nothing Then
            result = cm.cmisFaultType.CreateUnknownException()
         ElseIf result.Failure Is Nothing Then
            Return SerializeXmlSerializable(result.Success)
         End If

         'failure
         Throw LogException(result.Failure, serviceImpl)
      End Function
#End Region

#Region "5.2.9.2  Authentication with Tokens for Browser Clients"
      ''' <summary>
      ''' Throws an exception if authentication with token is enabled, but no valid token is sent
      ''' </summary>
      ''' <remarks></remarks>
      Private Function CheckTokenAuthentication(multipart As JSON.MultipartFormDataContent) As Token
         Dim sessionIdCookieName As String = CmisServiceImpl.GetSessionIdCookieName()
         Dim retVal As Token = If(CType(multipart, Token), CType(GetRequestParameter("token"), Token))

         If Not String.IsNullOrEmpty(sessionIdCookieName) Then
            Dim sessionIdCookie As String = If(CmisServiceImplBase.CurrentIncomingCookies.Value(sessionIdCookieName), String.Empty)

            If _tokenRequiringSessionIdCookies.Contains(sessionIdCookie.GetHashCode()) AndAlso
               (retVal Is Nothing OrElse Not retVal.Token.StartsWith(sessionIdCookie)) Then
               Throw LogException(Messaging.cmisFaultType.CreatePermissionDeniedException(), CmisServiceImpl)
            End If
         End If

         Return retVal
      End Function

      ''' <summary>
      ''' Within a sessionIdCookie supporting system this method checks if the hosting system added a sessionIdCookie
      ''' which signals a login process. If so the method stores the sessionId (from the outgoing cookie), if a token
      ''' query parameter is set by the client. Each following request from the client MUST support a valid token,
      ''' otherwise the request will be denied. This can prevent a form of cross-site request forgery.
      ''' </summary>
      ''' <remarks></remarks>
      Private Sub EnableTokenAuthentication()
         Dim sessionIdCookieName As String = CmisServiceImpl.GetSessionIdCookieName()
         Dim token As String = GetRequestParameter("token")

         If Not (String.IsNullOrEmpty(sessionIdCookieName) OrElse String.IsNullOrEmpty(token)) Then
            'the system supports sessionIdCookies and a token is set by the client to enable authentication with tokens
            '(see chapter 5.2.9.2 Authentication with Tokens for Browser Clients)
            Dim sessionIdCookie As String = CmisServiceImplBase.CurrentOutgoingCookies.Value(sessionIdCookieName)

            If Not String.IsNullOrEmpty(sessionIdCookie) Then _tokenRequiringSessionIdCookies.Add(sessionIdCookie.GetHashCode())
         End If
      End Sub

      Private Shared ReadOnly _tokenRequiringSessionIdCookies As New HashSet(Of Integer)
#End Region

      Private Shared _genericWebFaultExceptionTypeDefinition As Type = GetType(ssw.WebFaultException(Of String)).GetGenericTypeDefinition()

      ''' <summary>
      ''' Browser-binding extensions to allow cross-domain requests
      ''' </summary>
      ''' <param name="file"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function GetFile(file As JSON.enumJSONFile) As IO.MemoryStream
         Select Case file
            Case JSON.enumJSONFile.cmisJS
               Dim regEx As New System.Text.RegularExpressions.Regex("((?<FrameUri>" & Constants.jsFrameUri & ")" &
                                                                     "|(?<LoginKeyCookie>" & Constants.jsLoginKeyCookie & ")" &
                                                                     "|(?<RepositoryDomain>" & Constants.jsRepositoryDomain & ")" &
                                                                     "|(?<ServiceUri>" & Constants.jsServiceUri & ")" &
                                                                     "|(?<SessionIdCookieName>" & Constants.jsSessionIdCookie & ")" &
                                                                     ")",
                                                                     Text.RegularExpressions.RegexOptions.IgnoreCase Or Text.RegularExpressions.RegexOptions.Multiline)
               Dim baseUri As Uri = Me.BaseUri
               Dim relativeUri As String = ServiceURIs.RepositoriesUri(ServiceURIs.enumRepositoriesUri.file).ReplaceUri("file", "embeddedFrame.htm")
               Dim frameUri As Uri = baseUri.Combine(relativeUri)
               Dim loginKeyCookie As String = GetRequestParameter("loginKeyCookie")
               Dim serviceUri As Uri = If(ServiceURIs.GetRepositories = "/",
                                          If(baseUri.AbsoluteUri.EndsWith("/"), baseUri, New Uri(baseUri.AbsoluteUri & "/")),
                                          New Uri(baseUri, ServiceURIs.GetRepositories))
               Dim replacements As New Dictionary(Of String, String) From {
                  {"FrameUri", frameUri.AbsoluteUri}, {"LoginKeyCookie", loginKeyCookie}, {"RepositoryDomain", serviceUri.Authority},
                  {"ServiceUri", serviceUri.AbsoluteUri}, {"SessionIdCookieName", CmisServiceImpl.GetSessionIdCookieName()}}
               Dim evaluator As System.Text.RegularExpressions.MatchEvaluator =
                  Function(currentMatch)
                     For Each de As KeyValuePair(Of String, String) In replacements
                        Dim gr As System.Text.RegularExpressions.Group = currentMatch.Groups(de.Key)
                        If gr IsNot Nothing AndAlso gr.Success Then Return de.Value
                     Next
                     Return currentMatch.Value
                  End Function
               Return PrepareResult(regEx.Replace(My.Resources.cmis, evaluator), Constants.MediaTypes.JavaScript)
            Case JSON.enumJSONFile.embeddedFrameHtm
               'iFrame within cmis.js
               Dim relativeUri As String = ServiceURIs.RepositoriesUri(ServiceURIs.enumRepositoriesUri.cmisaction).ReplaceUri("cmisaction", "login")
               Dim uri As Uri = Me.BaseUri.Combine(relativeUri)
               Return PrepareResult(My.Resources.EmbeddedFrame.Replace(Constants.jsLoginUri, uri.AbsoluteUri), Constants.MediaTypes.Html)
            Case JSON.enumJSONFile.loginPageHtm
               'loginPage to log in into repository without Basic Authentication
               Dim regEx As New System.Text.RegularExpressions.Regex("((?<Host>" & Constants.jsHostingApplicationUri & ")" &
                                                                     "|(?<input>\<input\s+((?<name>[^\=\/\s]*)\s*\=\s*""(?<value>[^""]*)""\s*)+\s*\/\>))",
                                                                     Text.RegularExpressions.RegexOptions.IgnoreCase Or Text.RegularExpressions.RegexOptions.Multiline)
               Dim evaluator As System.Text.RegularExpressions.MatchEvaluator =
                  Function(currentMatch)
                     Dim grHost As System.Text.RegularExpressions.Group = currentMatch.Groups("Host")

                     If grHost IsNot Nothing AndAlso grHost.Success Then
                        Return GetRequestParameter("hostingApplicationUri")
                     Else
                        Dim dic As New Dictionary(Of String, String)
                        Dim grName As System.Text.RegularExpressions.Group = currentMatch.Groups("name")
                        Dim grValue As System.Text.RegularExpressions.Group = currentMatch.Groups("value")
                        For index As Integer = 0 To grName.Captures.Count - 1
                           dic.Add(grName.Captures(index).Value, grValue.Captures(index).Value)
                        Next
                        If dic.ContainsKey("name") Then
                           For Each inputName As String In New String() {"repositoryId", "user"}
                              If dic("name") = inputName Then
                                 dic.Remove("value")
                                 dic.Add("value", GetRequestParameter(inputName))
                                 Return "<input " & String.Join(" ", (From de As KeyValuePair(Of String, String) In dic
                                                                      Select de.Key & "=""" & de.Value & """").ToArray()) & " />"
                              End If
                           Next
                        End If

                        Return currentMatch.Value
                     End If
                  End Function
               Return PrepareResult(regEx.Replace(My.Resources.LoginPage, evaluator), Constants.MediaTypes.Html)
            Case JSON.enumJSONFile.loginRefPageHtm
               'give link to the login page
               Dim relativeUri As String = ServiceURIs.RepositoriesUri(ServiceURIs.enumRepositoriesUri.cmisaction).ReplaceUri("cmisaction", "login")
               Dim uri As Uri = Me.BaseUri.Combine(relativeUri)
               Return PrepareResult(My.Resources.LoginRefPage.Replace("LoginPage.htm", uri.AbsoluteUri), Constants.MediaTypes.Html)
            Case Else
               Return PrepareResult(My.Resources.EmptyPage, MediaTypes.Html)
         End Select
      End Function

      ''' <summary>
      ''' Returns folderId using following preferences:
      '''   1. return folderId if it is not null or empty
      '''   2. return folderId-querystring parameter if exist and not null or empty
      '''   3. return id-querystring parameter if exist and not null or empty
      '''   4. return rootFolderId of the repository
      ''' </summary>
      ''' <param name="serviceImpl"></param>
      ''' <param name="repositoryId"></param>
      ''' <param name="folderId"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function GetFolderId(serviceImpl As Contracts.ICmisServicesImpl,
                                   repositoryId As String, folderId As String) As String
         If String.IsNullOrEmpty(folderId) Then folderId = If(GetRequestParameter(ServiceURIs.enumCheckedOutUri.folderId), GetRequestParameter("id"))
         If String.IsNullOrEmpty(folderId) Then
            'rootfolder
            Dim repositoryInfo As Core.cmisRepositoryInfoType = serviceImpl.RepositoryInfo(repositoryId)
            If repositoryInfo IsNot Nothing Then folderId = repositoryInfo.RootFolderId
         End If

         Return folderId
      End Function

      ''' <summary>
      ''' Returns the response of a form request (POST)
      ''' </summary>
      ''' <remarks>see chapter 5.4.4.4 Access to Form Response Content of cmis documentation</remarks>
      Private Function GetFormResponse(cmisObject As Core.cmisObjectType,
                                       repositoryId As String,
                                       token As Token,
                                       statusCode As Net.HttpStatusCode,
                                       succinct As Boolean?) As IO.MemoryStream
         'all operations that return the HTTP status code 201 SHOULD also return a HTTP Location header (see 5.4 Services)
         If statusCode = Net.HttpStatusCode.Created Then
            Dim locationUri As Uri = Me.BaseUri.Combine(ServiceURIs.RootFolderUri(ServiceURIs.enumRootFolderUri.objectId).ReplaceUri("repositoryId", repositoryId, "id", cmisObject.ObjectId))
            ssw.WebOperationContext.Current.OutgoingResponse.Location = locationUri.AbsoluteUri
         End If
         If IsHtmlPageRequired(token) Then
            Dim transaction As New JSON.Transaction() With {.Code = statusCode, .Exception = Nothing, .Message = Nothing, .ObjectId = cmisObject.ObjectId}

            Me.Transaction(repositoryId, token) = transaction
            Return PrepareResult(My.Resources.EmptyPage, MediaTypes.Html)
         Else
            Return SerializeXmlSerializable(cmisObject, succinct, statusCode)
         End If
      End Function

      ''' <summary>
      ''' Returns the response of a form request (POST)
      ''' </summary>
      ''' <remarks>see chapter 5.4.4.4 Access to Form Response Content of cmis documentation</remarks>
      Private Function GetFormResponse(typeDefinition As Core.Definitions.Types.cmisTypeDefinitionType,
                                       repositoryId As String,
                                       token As Token,
                                       statusCode As Net.HttpStatusCode) As IO.MemoryStream
         'all operations that return the HTTP status code 201 SHOULD also return a HTTP Location header (see 5.4 Services)
         If statusCode = Net.HttpStatusCode.Created Then
            Dim locationUri As Uri = Me.BaseUri.Combine(ServiceURIs.RepositoryUri(ServiceURIs.enumRepositoryUri.typeId).ReplaceUri("repositoryId", repositoryId, "typeId", typeDefinition.Id))
            ssw.WebOperationContext.Current.OutgoingResponse.Location = locationUri.AbsoluteUri
         End If
         If IsHtmlPageRequired(token) Then
            Dim transaction As New JSON.Transaction() With {.Code = statusCode, .Exception = Nothing, .Message = Nothing, .ObjectId = typeDefinition.Id}

            Me.Transaction(repositoryId, token) = transaction
            Return PrepareResult(My.Resources.EmptyPage, MediaTypes.Html)
         Else
            Return SerializeXmlSerializable(typeDefinition, Nothing, statusCode)
         End If
      End Function

      ''' <summary>
      ''' Returns the typeDefinition declared for specified objectId (objectTypeId and secondaryTypeIds)
      ''' </summary>
      Private Function GetTypeDefinitions(serviceImpl As Contracts.ICmisServicesImpl, repositoryId As String, objectId As String) As Core.Definitions.Types.cmisTypeDefinitionType()
         Dim typeDefinitions As New List(Of Core.Definitions.Types.cmisTypeDefinitionType)

         'get current typeDefinitions (objectTypeId and secondaryTypeIds)
         Dim result = serviceImpl.GetObject(repositoryId, objectId, CmisPredefinedPropertyNames.ObjectTypeId & "," & CmisPredefinedPropertyNames.SecondaryObjectTypeIds,
                                            Core.enumIncludeRelationships.none, False, "cmis:none", False, False, Nothing, Nothing)
         If result IsNot Nothing AndAlso result.Failure Is Nothing AndAlso result.Success IsNot Nothing Then
            'collect typeIds
            Dim properties As Dictionary(Of String, Core.Properties.cmisProperty) = result.Success.GetProperties()

            For Each propertyName As String In New String() {CmisPredefinedPropertyNames.ObjectTypeId, CmisPredefinedPropertyNames.SecondaryObjectTypeIds}
               If properties.ContainsKey(propertyName) Then
                  Dim stringProperty As Core.Properties.Generic.cmisProperty(Of String) = TryCast(properties(propertyName), Core.Properties.Generic.cmisProperty(Of String))
                  Dim typeIds As String() = If(stringProperty Is Nothing, Nothing, stringProperty.Values)

                  If typeIds IsNot Nothing Then
                     For Each typeId As String In typeIds
                        Dim typeDefinition As Core.Definitions.Types.cmisTypeDefinitionType = serviceImpl.TypeDefinition(repositoryId, typeId)
                        If typeDefinition IsNot Nothing Then typeDefinitions.Add(typeDefinition)
                     Next
                  End If
               End If
            Next
         End If

         Return typeDefinitions.ToArray()
      End Function

      ''' <summary>
      ''' Returns True if a token control is used in a non ajax request 
      ''' </summary>
      ''' <param name="token"></param>
      ''' <returns></returns>
      ''' <remarks>Minor deviation to chapter 5.4.4.4.1 Client Implementation Hints of the cmis documentation, which explicitly defines:
      ''' whenever the token control is used, the repository must respond with a HTML page.
      ''' This implementation allows the usage of a token parameter within the querystring in POST methods (cmis documentation specifies
      ''' only a token control in a POST method). As a result clients can enable token authentication AND receive JSON objects from a
      ''' POST method.</remarks>
      Private Function IsHtmlPageRequired(token As Token) As Boolean
         Return token IsNot Nothing AndAlso token.Transmission = enumTokenTransmission.asControl AndAlso
                ssw.WebOperationContext.Current.IncomingRequest.Headers("X-Requested-With") <> "XMLHttpRequest"
      End Function

      ''' <summary>
      ''' Returns content as an utf-8 encoded stream
      ''' </summary>
      ''' <param name="content"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function PrepareResult(content As String, contentType As String) As IO.MemoryStream
         Dim buffer As Byte() = System.Text.Encoding.UTF8.GetBytes(content)
         Dim retVal As New IO.MemoryStream(buffer, 0, buffer.Length)

         retVal.Position = 0
         System.ServiceModel.Web.WebOperationContext.Current.OutgoingResponse.ContentType = contentType

         Return retVal
      End Function

      ''' <summary>
      ''' Contains the type used to represent a queryResultList
      ''' </summary>
      ''' <remarks></remarks>
      Private Shared _queryResultListType As Type

      ''' <summary>
      ''' Serializes an unspecific XmlSerializable-object-array
      ''' </summary>
      Private Function SerializeArray(Of TXmlSerializable As CmisObjectModel.Serialization.XmlSerializable)(objects As TXmlSerializable(),
                                                                                                            Optional succinct As Boolean? = Nothing,
                                                                                                            Optional statusCode As Net.HttpStatusCode = Net.HttpStatusCode.OK) As IO.MemoryStream
         Dim retVal As New IO.MemoryStream()
         '5.2.11  Succinct Representation of Properties
         Dim serializer As New JSON.Serialization.JavaScriptSerializer(succinct.HasValue AndAlso succinct.Value)

         'serialization for query (see Constants.JSONTypeDefinitions.RepresentationTypes("http://docs.oasis-open.org/ns/cmis/browser/201103/queryResultList"))
         If _queryResultListType.IsAssignableFrom(objects.GetType()) Then
            'respect the slightly differences of serializations for cmisObjects in queryResultLists
            serializer.AttributesOverrides.TypeConverter(GetType(Core.cmisObjectType)) = New JSON.Core.QueryResultConverter()
         End If

         'see chapter 5.2.8  Callback in cmis documentation
         Dim callback As String = GetRequestParameter("callback")
         Dim jsonString As String = If(objects Is Nothing, "[]", serializer.SerializeArray(objects))

         If String.IsNullOrEmpty(callback) Then
            Write(retVal, jsonString)
         Else
            Write(retVal, callback, "(", jsonString, ")")
         End If

         ssw.WebOperationContext.Current.OutgoingResponse.StatusCode = statusCode
         ssw.WebOperationContext.Current.OutgoingResponse.ContentType = If(String.IsNullOrEmpty(callback), MediaTypes.Json, MediaTypes.JavaScript)
         retVal.Position = 0

         Return retVal
      End Function

      ''' <summary>
      ''' Serializes an exception
      ''' </summary>
      ''' <param name="token">Set this parameter only in POST-requests</param>
      ''' <returns></returns>
      ''' <remarks>see chapter 5.2.10  Error Handling and Return Codes in cmis documentation</remarks>
      Private Function SerializeException(ex As Exception, Optional repositoryId As String = Nothing, Optional token As Token = Nothing) As IO.MemoryStream
         Dim exceptionType As Type = ex.GetType()
         Dim cmisFault As Messaging.cmisFaultType

         If exceptionType.IsGenericType AndAlso exceptionType.GetGenericTypeDefinition() Is _genericWebFaultExceptionTypeDefinition Then
            Dim grh As Common.GenericRuntimeHelper = Common.GenericRuntimeHelper.GetInstance(exceptionType.GetGenericArguments()(0))
            cmisFault = grh.CreateCmisFaultType(ex)
         ElseIf GetType(ssw.WebFaultException).IsAssignableFrom(exceptionType) Then
            cmisFault = Messaging.cmisFaultType.CreateInstance(CType(ex, ssw.WebFaultException))
         Else
            cmisFault = Messaging.cmisFaultType.CreateInstance(ex)
         End If

         If IsHtmlPageRequired(token) Then
            'POST-request with token
            Dim transaction As New JSON.Transaction() With {.Code = cmisFault.Code, .Exception = cmisFault.Type.GetName(), .Message = cmisFault.Message, .ObjectId = Nothing}
            Dim serializer As New JSON.Serialization.JavaScriptSerializer()

            Me.Transaction(repositoryId, token) = transaction
            ssw.WebOperationContext.Current.OutgoingResponse.StatusCode = Net.HttpStatusCode.OK
            Return PrepareResult(My.Resources.EmptyPage, MediaTypes.Html)
         Else
            'GET-request or POST-request without token
            Dim suppressResponseCodes As Boolean? = ParseBoolean(GetRequestParameter("suppressResponseCodes"))

            Return SerializeXmlSerializable(cmisFault, Nothing,
                                            If(suppressResponseCodes.HasValue AndAlso suppressResponseCodes.Value,
                                               Net.HttpStatusCode.OK, CType(cmisFault.Code, Net.HttpStatusCode)))
         End If
      End Function

      ''' <summary>
      ''' Serializes an array of cmisRepositoryInfoType-objects as utf-8 encoded JSON-stream
      ''' </summary>
      ''' <param name="repositories"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function SerializeRepositories(repositories As Core.cmisRepositoryInfoType()) As IO.MemoryStream
         If repositories Is Nothing Then
            'statuscode already set!
            Return PrepareResult(My.Resources.EmptyPage, MediaTypes.Html)
         Else
            Dim retVal As New IO.MemoryStream
            Dim baseUri As Uri = Me.BaseUri
            Dim serializer As New JSON.Serialization.JavaScriptSerializer()
            'see chapter 5.2.8  Callback in cmis documentation
            Dim callback As String = GetRequestParameter("callback")

            'add URLs (see chapter 5.3.1  Service URL in the cmis documentation)
            For Each repository As Core.cmisRepositoryInfoType In repositories
               If repository IsNot Nothing Then
                  Dim uri As Uri = baseUri.Combine(ServiceURIs.RepositoryUri(ServiceURIs.enumRepositoryUri.none).ReplaceUri("repositoryId", repository.RepositoryId))
                  repository.RepositoryUrl = uri.AbsoluteUri
                  uri = baseUri.Combine(ServiceURIs.RootFolder.ReplaceUri("repositoryId", repository.RepositoryId))
                  repository.RootFolderUrl = uri.AbsoluteUri
               End If
            Next
            Dim jsonString As String = serializer.SerializeMap(repositories, Core.cmisRepositoryInfoType.DefaultKeyProperty)
            If String.IsNullOrEmpty(callback) Then
               Write(retVal, jsonString)
            Else
               Write(retVal, callback, "(", jsonString, ")")
            End If

            ssw.WebOperationContext.Current.OutgoingResponse.StatusCode = Net.HttpStatusCode.OK
            ssw.WebOperationContext.Current.OutgoingResponse.ContentType = If(String.IsNullOrEmpty(callback), MediaTypes.Json, MediaTypes.JavaScript)
            retVal.Position = 0

            Return retVal
         End If
      End Function

      ''' <summary>
      ''' Serializes an unspecific XmlSerializable-object
      ''' </summary>
      Private Function SerializeXmlSerializable(Of TXmlSerializable As CmisObjectModel.Serialization.XmlSerializable)(obj As TXmlSerializable,
                                                                                                                      Optional succinct As Boolean? = Nothing,
                                                                                                                      Optional statusCode As Net.HttpStatusCode = Net.HttpStatusCode.OK) As IO.MemoryStream
         Dim retVal As New IO.MemoryStream()
         Dim serializer As New JSON.Serialization.JavaScriptSerializer()

         '5.2.11  Succinct Representation of Properties
         If succinct.HasValue AndAlso succinct.Value Then
            serializer.AttributesOverrides.ElementAttribute(GetType(Core.cmisObjectType), "properties") =
               New JSON.Serialization.JSONAttributeOverrides.JSONElementAttribute("succinctProperties", New JSON.Collections.SuccinctPropertiesConverter())
         End If
         'serialization for query (see Constants.JSONTypeDefinitions.RepresentationTypes("http://docs.oasis-open.org/ns/cmis/browser/201103/queryResultList"))
         If _queryResultListType.IsAssignableFrom(obj.GetType()) Then
            'respect the slightly differences of serializations for cmisObjects in queryResultLists
            serializer.AttributesOverrides.TypeConverter(GetType(Core.cmisObjectType)) = New JSON.Core.QueryResultConverter()
         End If

         Dim jsonString As String = If(obj Is Nothing, "{}", serializer.Serialize(obj))

         'see chapter 5.2.8  Callback in cmis documentation
         If Not QueryParameterExists("callback") Then
            Write(retVal, jsonString)
            ssw.WebOperationContext.Current.OutgoingResponse.ContentType = MediaTypes.Json
         Else
            Dim callback As String = GetRequestParameter("callback")

            If String.IsNullOrEmpty(callback) Then
               Throw LogException(cm.cmisFaultType.CreateInvalidArgumentException("If queryparameter 'callback' is defined it MUST NOT be null.", False), CmisServiceImpl)
            Else
               Write(retVal, callback, "(", jsonString, ")")
               ssw.WebOperationContext.Current.OutgoingResponse.ContentType = MediaTypes.JavaScript
            End If
         End If

         ssw.WebOperationContext.Current.OutgoingResponse.StatusCode = statusCode
         retVal.Position = 0

         Return retVal
      End Function

      ''' <summary>
      ''' Gets or sets lastResultCookie for specified token
      ''' </summary>
      Private ReadOnly Property Transaction(token As Token) As JSON.Transaction
         Get
            Dim key As String = "lastResultCookie" & If(token Is Nothing, String.Empty, token.Token).GetHashCode()
            Dim cookie As HttpCookie = CmisServiceImplBase.CurrentIncomingCookies.Cookie(key)

            If cookie Is Nothing Then
               Return Nothing
            Else
               Dim serializer As New JSON.Serialization.JavaScriptSerializer()
               Try
                  Return serializer.Deserialize(Of JSON.Transaction)(System.Net.WebUtility.HtmlDecode(cookie.Value))
               Catch ex As Exception
                  Return Nothing
               End Try
            End If
         End Get
      End Property
      Private WriteOnly Property Transaction(repositoryId As String, token As Token) As JSON.Transaction
         Set(value As JSON.Transaction)
            Dim currentDate As DateTimeOffset = DateTimeOffset.Now
            Dim maxAge As Integer = If(value Is Nothing, 0, 3600)
            Dim expires As DateTime = DateTime.UtcNow.AddHours(If(maxAge = 0, -1.0, 1.0))
            Dim cookieValue As String = If(value Is Nothing, "_",
                                           (New JSON.Serialization.JavaScriptSerializer()).Serialize(value))
            Dim cookie As New HttpCookie("lastResultCookie" & If(token Is Nothing, String.Empty, token.Token).GetHashCode(),
                                         cookieValue) With {
                                         .Expires = expires, .MaxAge = maxAge, .Path = BaseUri.Combine(ServiceURIs.RepositoryUri(ServiceURIs.enumRepositoryUri.none).ReplaceUri("repositoryId", repositoryId)).AbsolutePath}
            CmisServiceImplBase.AddOutgoingCookie(cookie)
         End Set
      End Property

      ''' <summary>
      ''' Writes value as utf-8 encoded buffer into ms
      ''' </summary>
      Private Sub Write(destination As IO.MemoryStream, ParamArray values As String())
         For Each value As String In values
            If Not String.IsNullOrEmpty(value) Then
               Dim buffer As Byte() = System.Text.Encoding.UTF8.GetBytes(value)
               destination.Write(buffer, 0, buffer.Length)
            End If
         Next
      End Sub
   End Class
End Namespace