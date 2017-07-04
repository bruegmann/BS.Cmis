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
Imports cm = CmisObjectModel.Messaging
Imports ss = System.ServiceModel
Imports ssp = System.Security.Principal
Imports sss = System.ServiceModel.Syndication
Imports ssw = System.ServiceModel.Web
Imports sw = System.Web
Imports sws = System.Web.Services
Imports swsp = System.Web.Services.Protocols

Namespace CmisObjectModel.ServiceModel.WebService
   ''' <summary>
   ''' Cmis Webservice-implementation
   ''' </summary>
   ''' <remarks>under construction</remarks>
   <sws.WebService(Namespace:=Namespaces.cmisw, Description:="CMIS-WebService"),
    sws.WebServiceBinding(ConformsTo:=sws.WsiProfiles.BasicProfile1_1),
    ss.XmlSerializerFormat(SupportFaults:=True)>
   Public Class CmisWebService
      Inherits sws.WebService

#Region "Repository"
      <sws.WebMethod(enableSession:=False, Description:="Creates a new type in the repository"),
       ss.OperationContract(),
       ss.FaultContract(GetType(Messaging.cmisFaultType), Name:="cmisFault", Namespace:=Namespaces.cmism)>
      Public Function CreateType(request As cm.Requests.createType) As cm.Responses.createTypeResponse
         Dim response As cm.Responses.createTypeResponse = CmisWebServiceImpl.CreateType(request)

         If response Is Nothing Then
            Throw cm.cmisFaultType.CreateUnknownException()
         Else
            Return response
         End If
      End Function
#End Region

      ''' <summary>
      ''' Encapsulates the identity of requesting user as SyndicationPerson
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property Author As sss.SyndicationPerson
         Get
            Dim user As ssp.IPrincipal = If(Me.Context Is Nothing, Nothing, Me.Context.User)
            Dim identity As ssp.IIdentity = If(user Is Nothing, Nothing, user.Identity)
            Dim userName As String = If(identity Is Nothing, Nothing, identity.Name)

            Return If(String.IsNullOrEmpty(userName), Nothing, New sss.SyndicationPerson(Nothing, userName, Nothing))
         End Get
      End Property

      ''' <summary>
      ''' Encapsulates the identity of requesting user as SyndicationPerson()
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property Authors As sss.SyndicationPerson()
         Get
            Dim author As sss.SyndicationPerson = Me.Author
            Return If(author Is Nothing, Nothing, New sss.SyndicationPerson() {author})
         End Get
      End Property

      Public ReadOnly Property BaseUri As Uri
         Get
            Return New Uri(Me.Context.Request.RawUrl)
         End Get
      End Property

      Private _cmisServiceImplFactory As Generic.ServiceImplFactory(Of CmisWebServiceImplBase)
      ''' <summary>
      ''' Returns ICmisService-instance that implements the services declared in cmis
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private ReadOnly Property CmisWebServiceImpl As CmisWebServiceImplBase
         Get
            If _cmisServiceImplFactory Is Nothing Then _cmisServiceImplFactory = New Generic.ServiceImplFactory(Of CmisWebServiceImplBase)(BaseUri)
            Return _cmisServiceImplFactory.CmisServiceImpl
         End Get
      End Property
   End Class
End Namespace