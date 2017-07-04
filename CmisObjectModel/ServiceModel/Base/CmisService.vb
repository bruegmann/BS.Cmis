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
Imports ccg = CmisObjectModel.Common.Generic
Imports cm = CmisObjectModel.Messaging
Imports ss = System.ServiceModel
Imports ssw = System.ServiceModel.Web

'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.ServiceModel.Base
   Public MustInherit Class CmisService

#Region "Repository"
      ''' <summary>
      ''' Returns all available repositories
      ''' </summary>
      Protected Function GetRepositories(Of TResult)(fnSuccess As Func(Of Core.cmisRepositoryInfoType(), TResult)) As TResult
         Dim serviceImpl = CmisServiceImpl
         'enable possibility to specify repositoryId through queryString
         Dim repositoryId As String = GetRequestParameter("repositoryId")

         If String.IsNullOrEmpty(repositoryId) Then
            Dim result As ccg.Result(Of Core.cmisRepositoryInfoType())

            Try
               result = serviceImpl.GetRepositories()
               If result.Failure Is Nothing Then
                  Return fnSuccess(If(result.Success, New Core.cmisRepositoryInfoType() {}))
               End If
            Catch ex As Exception
               If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
                  serviceImpl.LogException(ex)
#End If
                  Throw
               Else
                  result = cm.cmisFaultType.CreateUnknownException(ex)
               End If
            End Try

            'failure
            Throw LogException(result.Failure, serviceImpl)
         Else
            Return GetRepositoryInfo(repositoryId, fnSuccess)
         End If
      End Function

      ''' <summary>
      ''' Returns the specified repository
      ''' </summary>
      Protected Function GetRepositoryInfo(Of TResult)(repositoryId As String,
                                                       fnSuccess As Func(Of Core.cmisRepositoryInfoType(), TResult)) As TResult
         If String.IsNullOrEmpty(repositoryId) Then
            'if there is no repositoryId specified return all available repositories
            Return GetRepositories(Of TResult)(fnSuccess)
         Else
            Dim serviceImpl = CmisServiceImpl
            Dim result As ccg.Result(Of Core.cmisRepositoryInfoType)
            Dim queryParameters As System.Collections.Generic.Dictionary(Of String, String)

            Try
               queryParameters = GetRequestParameters()
               If queryParameters.ContainsKey("logout") Then
                  With CmisServiceImpl.Logout(repositoryId)
                     If .Failure Is Nothing Then
                        ssw.WebOperationContext.Current.OutgoingResponse.StatusCode = .Success
                        Return fnSuccess(Nothing)
                     Else
                        result = .Failure
                     End If
                  End With
               ElseIf queryParameters.ContainsKey("ping") Then
                  With CmisServiceImpl.Ping(repositoryId)
                     If .Failure Is Nothing Then
                        ssw.WebOperationContext.Current.OutgoingResponse.StatusCode = .Success
                        Return fnSuccess(Nothing)
                     Else
                        result = .Failure
                     End If
                  End With
               Else
                  result = serviceImpl.GetRepositoryInfo(repositoryId)

                  If result Is Nothing Then
                     result = cm.cmisFaultType.CreateUnknownException()
                  ElseIf result.Failure Is Nothing Then
                     Dim repositoryInfo As Core.cmisRepositoryInfoType = result.Success
                     If repositoryInfo Is Nothing Then
                        Return fnSuccess(New Core.cmisRepositoryInfoType() {})
                     Else
                        Return fnSuccess(New Core.cmisRepositoryInfoType() {repositoryInfo})
                     End If
                  End If
               End If
            Catch ex As Exception
               If IsWebException(ex) Then
#If EnableExceptionLogging = "True" Then
                  serviceImpl.LogException(ex)
#End If
                  Throw
               Else
                  result = cm.cmisFaultType.CreateUnknownException(ex)
               End If
            End Try

            'failure
            Throw LogException(result.Failure, serviceImpl)
         End If
      End Function
#End Region

      ''' <summary>
      ''' BaseUri of the CMIS service
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property BaseUri As Uri
         Get
            Return ss.OperationContext.Current.EndpointDispatcher.EndpointAddress.Uri
         End Get
      End Property

      Private _cmisServiceImplFactory As Generic.ServiceImplFactory(Of Contracts.ICmisServicesImpl)
      ''' <summary>
      ''' Returns ICmisService-instance that implements the services declared in cmis
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected ReadOnly Property CmisServiceImpl As Contracts.ICmisServicesImpl
         Get
            If _cmisServiceImplFactory Is Nothing Then _cmisServiceImplFactory = New Generic.ServiceImplFactory(Of Contracts.ICmisServicesImpl)(BaseUri)
            Return _cmisServiceImplFactory.CmisServiceImpl
         End Get
      End Property

      ''' <summary>
      ''' Logs an exception before it will be thrown
      ''' </summary>
      ''' <param name="ex"></param>
      ''' <param name="serviceImpl"></param>
      ''' <returns></returns>
      ''' <remarks>Compiler constant EnableExceptionLogging must be set to 'True'</remarks>
      Protected Function LogException(ex As Exception, serviceImpl As Contracts.ICmisServicesImpl) As Exception
#If EnableExceptionLogging = "True" Then
         serviceImpl.LogException(ex)
#End If
         If ssw.WebOperationContext.Current.OutgoingResponse.StatusCode = Net.HttpStatusCode.OK Then
            If TypeOf ex Is ssw.WebFaultException(Of cm.cmisFaultType) Then
               ssw.WebOperationContext.Current.OutgoingResponse.StatusCode =
                  CType(ex, ssw.WebFaultException(Of cm.cmisFaultType)).StatusCode
            Else
               ssw.WebOperationContext.Current.OutgoingResponse.StatusCode = Net.HttpStatusCode.InternalServerError
            End If
         End If
         Return ex
      End Function

      Private Shared _genericWebFaultExceptionType As Type = GetType(ssw.WebFaultException(Of String)).GetGenericTypeDefinition()
      ''' <summary>
      ''' Returns True if ex based on WebFaultException or WebFaultException(Of T)
      ''' </summary>
      ''' <param name="ex"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Shared Function IsWebException(ex As Exception) As Boolean
         Return (TypeOf ex Is ssw.WebFaultException) OrElse ex.GetType.BasedOnGenericTypeDefinition(_genericWebFaultExceptionType)
      End Function

      ''' <summary>
      ''' Returns True if parameterName exists in the key list of the queryparameters within the current request
      ''' </summary>
      ''' <param name="parameterName"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Function QueryParameterExists(parameterName As String) As Boolean
         Dim requestParams As System.Collections.Specialized.NameValueCollection = If(ssw.WebOperationContext.Current Is Nothing, Nothing,
                                                                                      ssw.WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters)
         If requestParams Is Nothing Then
            Return False
         Else
            For Each key As String In requestParams.AllKeys
               If String.Compare(key, parameterName, True) = 0 Then Return True
            Next
         End If

         Return False
      End Function
   End Class
End Namespace