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
Imports sc = System.ComponentModel

Namespace CmisObjectModel.ComponentModel
   ''' <summary>
   ''' PropertyChangedEventArgs with added informations for old and new value
   ''' </summary>
   ''' <remarks></remarks>
   Public MustInherit Class PropertyChangedEventArgs
      Inherits sc.PropertyChangedEventArgs

      Protected Sub New(propertyName As String)
         MyBase.New(propertyName)
      End Sub

      Public ReadOnly Property NewValue As Object
         Get
            Return NewValueCore
         End Get
      End Property
      Protected MustOverride ReadOnly Property NewValueCore As Object

      Public ReadOnly Property OldValue As Object
         Get
            Return OldValueCore
         End Get
      End Property
      Protected MustOverride ReadOnly Property OldValueCore As Object
   End Class

   Namespace Generic
      Public Class PropertyChangedEventArgs(Of TProperty)
         Inherits CmisObjectModel.ComponentModel.PropertyChangedEventArgs

         Public Sub New(propertyName As String, newValue As TProperty, oldValue As TProperty)
            MyBase.New(propertyName)
            _newValue = newValue
            _oldValue = oldValue
         End Sub

         Private _newValue As TProperty
         Public Shadows ReadOnly Property NewValue As TProperty
            Get
               Return _newValue
            End Get
         End Property
         Protected Overrides ReadOnly Property NewValueCore As Object
            Get
               Return _newValue
            End Get
         End Property

         Private _oldValue As TProperty
         Public Shadows ReadOnly Property OldValue As TProperty
            Get
               Return _oldValue
            End Get
         End Property
         Protected Overrides ReadOnly Property OldValueCore As Object
            Get
               Return _oldValue
            End Get
         End Property
      End Class
   End Namespace
End Namespace