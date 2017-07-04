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
Imports ss = System.ServiceModel
Imports ssw = System.ServiceModel.Web

Namespace CmisObjectModel.Contracts
   ''' <summary>
   ''' CMIS-BrowserBinding services supported in this assembly
   ''' </summary>
   ''' <remarks>
   ''' WCF Service
   ''' see http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/CMIS-v1.1-cs01.html
   ''' 5.3 URLs</remarks>
   <ss.ServiceContract(SessionMode:=ss.SessionMode.NotAllowed, Namespace:=Namespaces.cmisw)>
   Public Interface IBrowserBinding

#Region "5.3.1. Service URL"
      <ss.OperationContract(Name:="dispatchWebGetService"),
       ssw.WebGet(UriTemplate:=ServiceURIs.GetRepositories, ResponseFormat:=System.ServiceModel.Web.WebMessageFormat.Json)>
      Function DispatchWebGetService() As IO.Stream

      <ss.OperationContract(Name:="dispatchWebPostService"),
       ssw.WebInvoke(Method:="POST", UriTemplate:=ServiceURIs.GetRepositories, ResponseFormat:=System.ServiceModel.Web.WebMessageFormat.Json)>
      Function DispatchWebPostService(stream As IO.Stream) As IO.Stream
#End Region

#Region "5.3.2 Repository URL"
      <ss.OperationContract(Name:="dispatchWebGetRepository"),
       ssw.WebGet(UriTemplate:=ServiceURIs.GetRepositoryInfo, ResponseFormat:=System.ServiceModel.Web.WebMessageFormat.Json)>
      Function DispatchWebGetRepository(repositoryId As String) As IO.Stream

      <ss.OperationContract(Name:="dispatchWebPostRepository"),
       ssw.WebInvoke(Method:="POST", UriTemplate:=ServiceURIs.GetRepositoryInfo, ResponseFormat:=System.ServiceModel.Web.WebMessageFormat.Json)>
      Function DispatchWebPostRepository(repositoryId As String, stream As IO.Stream) As IO.Stream
#End Region

#Region "5.3.3 Root Folder URL"
      <ss.OperationContract(Name:="dispatchWebGetRootFolder"),
       ssw.WebGet(UriTemplate:=ServiceURIs.RootFolder & "?objectId={objectId}", ResponseFormat:=System.ServiceModel.Web.WebMessageFormat.Json)>
      Function DispatchWebGetRootFolder(repositoryId As String, objectId As String) As IO.Stream

      <ss.OperationContract(Name:="dispatchWebPostRootFolder"),
       ssw.WebInvoke(Method:="POST", UriTemplate:=ServiceURIs.RootFolder & "?objectId={objectId}", ResponseFormat:=System.ServiceModel.Web.WebMessageFormat.Json)>
      Function DispatchWebPostRootFolder(repositoryId As String, objectId As String, stream As IO.Stream) As IO.Stream
#End Region

#Region "5.3.4 Object URL"
      <ss.OperationContract(Name:="dispatchWebGetObjects"),
       ssw.WebGet(UriTemplate:=ServiceURIs.AbsoluteObject, ResponseFormat:=System.ServiceModel.Web.WebMessageFormat.Json)>
      Function DispatchWebGetObjects(repositoryId As String, path As String) As IO.Stream

      <ss.OperationContract(Name:="dispatchWebPostObjects"),
       ssw.WebInvoke(Method:="POST", UriTemplate:=ServiceURIs.AbsoluteObject, ResponseFormat:=System.ServiceModel.Web.WebMessageFormat.Json)>
      Function DispatchWebPostObjects(repositoryId As String, path As String, stream As IO.Stream) As IO.Stream
#End Region

   End Interface
End Namespace