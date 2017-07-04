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

Namespace CmisObjectModel.Messaging
   Partial Public Class cmisObjectInFolderListType
      Implements IEnumerable(Of Core.cmisObjectType)

#Region "IEnumerable"
      Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of Core.cmisObjectType) Implements System.Collections.Generic.IEnumerable(Of Core.cmisObjectType).GetEnumerator
         Return (From cmisObjectObjectInFolder As Messaging.cmisObjectInFolderType In If(_objects, New Messaging.cmisObjectInFolderType() {})
                 Let cmisObject As Core.cmisObjectType = If(cmisObjectObjectInFolder Is Nothing, Nothing, cmisObjectObjectInFolder.Object)
                 Where cmisObject IsNot Nothing
                 Select cmisObject).GetEnumerator()
      End Function

      Protected Overridable Function IEnumerable_GetEnumerator() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
         Return GetEnumerator()
      End Function
#End Region

      ''' <summary>
      ''' Converts AtomFeed of objects
      ''' </summary>
      ''' <param name="value"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Widening Operator CType(value As AtomPub.AtomFeed) As cmisObjectInFolderListType
         If value Is Nothing Then
            Return Nothing
         Else
            Dim objects As List(Of cmisObjectInFolderType) =
               (From entry As AtomPub.AtomEntry In If(value.Entries, New List(Of AtomPub.AtomEntry))
                Let [object] As cmisObjectInFolderType = entry
                Where [object] IsNot Nothing
                Select [object]).ToList()
            Return New cmisObjectInFolderListType() With {._hasMoreItems = value.HasMoreItems, ._numItems = value.NumItems,
                                                          ._objects = If(objects.Count = 0, Nothing, objects.ToArray())}
         End If
      End Operator

   End Class
End Namespace