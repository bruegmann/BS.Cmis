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
   Public MustInherit Class CmisWebServiceImplBase

      Private _baseUri As Uri
      Protected Sub New(baseUri As Uri)
         _baseUri = baseUri
      End Sub

#Region "Repository"
      Public MustOverride Function CreateType(request As cm.Requests.createType) As cm.Responses.createTypeResponse
#End Region

   End Class
End Namespace