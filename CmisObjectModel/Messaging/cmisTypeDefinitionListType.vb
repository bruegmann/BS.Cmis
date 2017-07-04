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
   Partial Public Class cmisTypeDefinitionListType

      ''' <summary>
      ''' Converts AtomFeed of typedefinitions
      ''' </summary>
      ''' <param name="value"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Widening Operator CType(value As AtomPub.AtomFeed) As cmisTypeDefinitionListType
         If value Is Nothing Then
            Return Nothing
         Else
            Dim types As List(Of Core.Definitions.Types.cmisTypeDefinitionType) =
               (From entry As AtomPub.AtomEntry In If(value.Entries, New List(Of AtomPub.AtomEntry))
                Let type As Core.Definitions.Types.cmisTypeDefinitionType = If(entry Is Nothing, Nothing, entry.Type)
                Where type IsNot Nothing
                Select type).ToList()
            Return New cmisTypeDefinitionListType() With {._hasMoreItems = value.HasMoreItems, ._numItems = value.NumItems,
                                                          ._types = If(types.Count = 0, Nothing, types.ToArray())}
         End If
      End Operator

   End Class
End Namespace