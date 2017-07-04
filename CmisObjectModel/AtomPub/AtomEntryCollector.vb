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

Namespace CmisObjectModel.AtomPub
   ''' <summary>
   ''' A simple collector of AtomEntry-objects in a tree
   ''' </summary>
   ''' <remarks></remarks>
   Public Class AtomEntryCollector
      Inherits Common.Generic.TreeEntryCollector(Of AtomFeed, AtomEntry)

      Public Sub New()
         MyBase.New()
      End Sub

      Protected Overrides Function GetChildren(entry As AtomEntry) As AtomFeed
         Return If(entry Is Nothing, Nothing, entry.Children)
      End Function

      Protected Overrides Function GetEntries(list As AtomFeed) As System.Collections.Generic.IEnumerable(Of AtomEntry)
         Return If(list Is Nothing, Nothing, list.Entries)
      End Function
   End Class
End Namespace