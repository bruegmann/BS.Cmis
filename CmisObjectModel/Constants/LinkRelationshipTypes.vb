'*******************************************************************************************
'* Copyright Brügmann Software GmbH, Papenburg
'* Author: Björn Kremer
'* Contact: codeplex<at>patorg.de
'* 
'* VB.CMIS is a VB.NET implementation of the Content Management Interoperability Services (CMIS) standard
'*
'* This file is part of VB.CMIS.
'* 
'* VB.CMIS is free software: you can redistribute it and/or modify
'* it under the terms of the GNU Lesser General Public License as published by
'* the Free Software Foundation, either version 3 of the License, or
'* (at your option) any later version.
'* 
'* VB.CMIS is distributed in the hope that it will be useful,
'* but WITHOUT ANY WARRANTY; without even the implied warranty of
'* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'* GNU Lesser General Public License for more details.
'* 
'* You should have received a copy of the GNU Lesser General Public License
'* along with VB.CMIS. If not, see <http://www.gnu.org/licenses/>.
'*******************************************************************************************

Namespace CmisObjectModel.Constants
   Public MustInherit Class LinkRelationshipTypes
      Private Sub New()
      End Sub

      Public Const Alternate As String = "alternate"
      Public Const CurrentVersion As String = "current-version"
      Public Const DescribedBy As String = "describedby"
      Public Const Down As String = "down"
      Public Const Edit As String = "edit"
      Public Const EditMedia As String = "edit-media"
      Public Const Enclosure As String = "enclosure"
      Public Const First As String = "first"
      Public Const Last As String = "last"
      Public Const [Next] As String = "next"
      Public Const Previous As String = "previous"
      Public Const Self As String = "self"
      Public Const Service As String = "service"
      Public Const Up As String = "up"
      Public Const Via As String = "via"
      Public Const VersionHistory As String = "version-history"
      Public Const WorkingCopy As String = "working-copy"

      Public Const Acl As String = "http://docs.oasis-open.org/ns/cmis/link/200908/acl"
      Public Const AllowableActions As String = "http://docs.oasis-open.org/ns/cmis/link/200908/allowableactions"
      Public Const Changes As String = "http://docs.oasis-open.org/ns/cmis/link/200908/changes"
      Public Const FolderTree As String = "http://docs.oasis-open.org/ns/cmis/link/200908/foldertree"
      Public Const Policies As String = "http://docs.oasis-open.org/ns/cmis/link/200908/policies"
      Public Const Relationships As String = "http://docs.oasis-open.org/ns/cmis/link/200908/relationships"
      Public Const RootDescendants As String = "http://docs.oasis-open.org/ns/cmis/link/200908/rootdescendants"
      Public Const Source As String = "http://docs.oasis-open.org/ns/cmis/link/200908/source"
      Public Const Target As String = "http://docs.oasis-open.org/ns/cmis/link/200908/target"
      Public Const TypeDescendants As String = "http://docs.oasis-open.org/ns/cmis/link/200908/typedescendants"

      'link to the content-stream of a document
      Public Const ContentStream = ServiceURIs.GetContentStream
   End Class
End Namespace