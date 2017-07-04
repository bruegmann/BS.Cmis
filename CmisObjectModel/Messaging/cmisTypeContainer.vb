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
   Partial Public Class cmisTypeContainer

      ''' <summary>
      ''' Converts AtomEntry
      ''' </summary>
      ''' <param name="value"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Widening Operator CType(value As AtomPub.AtomEntry) As cmisTypeContainer
         Return Convert(value)
      End Operator

      ''' <summary>
      ''' Converts AtomEntry
      ''' </summary>
      ''' <param name="value"></param>
      ''' <returns></returns>
      ''' <remarks>Method implemented to avoid warning within the Operator CType(AtomEntry): recursion!</remarks>
      Public Shared Function Convert(value As AtomPub.AtomEntry) As cmisTypeContainer
         Dim type As Core.Definitions.Types.cmisTypeDefinitionType = If(value Is Nothing, Nothing, value.Type)

         If type Is Nothing Then
            Return Nothing
         Else
            Dim childrenFeed As AtomPub.AtomFeed = value.Children
            Dim children As List(Of cmisTypeContainer) =
               (From entry As AtomPub.AtomEntry In If(If(childrenFeed Is Nothing, Nothing, childrenFeed.Entries), New List(Of AtomPub.AtomEntry))
                Let child As cmisTypeContainer = Convert(entry)
                Where child IsNot Nothing
                Select child).ToList()
            Return New cmisTypeContainer() With {._children = If(children.Count = 0, Nothing, children.ToArray()), ._type = type}
         End If
      End Function

   End Class
End Namespace