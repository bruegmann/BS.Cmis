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
Imports CmisObjectModel.Constants
Imports CmisObjectModel.Core.Collections

Namespace CmisObjectModel.Client.Vendors
   Public Class Agorum
      Inherits Vendor

#Region "Constructors"
      Public Sub New(client As Contracts.ICmisClient)
         MyBase.New(client)
      End Sub
#End Region

#Region "Patches"
      ''' <summary>
      ''' In agorum the cmis:versionSeriesId property of the pwc differs from the cmis:versionSeriesId
      ''' of the checkedin-versions of the document.
      ''' </summary>
      ''' <param name="properties"></param>
      Public Overrides Sub PatchProperties(repositoryInfo As Core.cmisRepositoryInfoType, properties As cmisPropertiesType)
         If properties IsNot Nothing Then
            Try
               With properties.GetProperties(CmisPredefinedPropertyNames.ObjectId,
                                             CmisPredefinedPropertyNames.VersionSeriesCheckedOutId,
                                             CmisPredefinedPropertyNames.VersionSeriesId)
                  If .Count = 3 Then
                     Dim versionSeriesIdProperty As Core.Properties.cmisProperty = .Item(Constants.CmisPredefinedPropertyNames.VersionSeriesId)
                     Dim objectId As String = CStr(.Item(Constants.CmisPredefinedPropertyNames.ObjectId).Value)
                     'check if properties belong to a pwc
                     If Not String.IsNullOrEmpty(objectId) AndAlso
                        String.Compare(objectId, CStr(.Item(CmisPredefinedPropertyNames.VersionSeriesCheckedOutId).Value)) = 0 Then
                        Dim filter As String = String.Join(",", CmisPredefinedPropertyNames.ObjectId,
                                                           CmisPredefinedPropertyNames.VersionSeriesId)

                        With _client.GetObjectOfLatestVersion(New Messaging.Requests.getObjectOfLatestVersion() With {
                                                              .Filter = filter, .ObjectId = objectId, .RepositoryId = repositoryInfo.RepositoryId})
                           If .Exception Is Nothing AndAlso .Response IsNot Nothing Then
                              'replace value of cmis:versionSeriesId property of pwc with the
                              'value returned by the last checkedin version of the document
                              Dim versionSeriesId As String = .Response.Object.VersionSeriesId.Value
                              If Not String.IsNullOrEmpty(versionSeriesId) Then versionSeriesIdProperty.Value = versionSeriesId
                           End If
                        End With
                     End If
                  End If
               End With
            Catch
            End Try
         End If
         MyBase.PatchProperties(repositoryInfo, properties)
      End Sub
#End Region

   End Class
End Namespace