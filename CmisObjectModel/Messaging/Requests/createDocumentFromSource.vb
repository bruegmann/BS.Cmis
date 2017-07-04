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
Imports ssw = System.ServiceModel.Web

Namespace CmisObjectModel.Messaging.Requests
   Partial Public Class createDocumentFromSource

      ''' <summary>
      ''' Reads transmitted parameters from queryString
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <remarks></remarks>
      Public Overrides Sub ReadQueryString(repositoryId As String)
         Dim requestParams As System.Collections.Specialized.NameValueCollection = If(ssw.WebOperationContext.Current Is Nothing, Nothing,
                                                                                      ssw.WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters)

         MyBase.ReadQueryString(repositoryId)

         If requestParams IsNot Nothing Then
            _repositoryId = Read(repositoryId, _repositoryId)
            _sourceId = Read(requestParams("sourceId"), _sourceId)
            _folderId = Read(requestParams("folderId"), _folderId)
            _versioningState = ReadOptionalEnum(requestParams("versioningState"), _versioningState)
         End If
      End Sub

   End Class
End Namespace