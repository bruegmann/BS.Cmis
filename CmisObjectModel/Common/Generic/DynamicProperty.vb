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

Namespace CmisObjectModel.Common.Generic
   ''' <summary>
   ''' Encapsulation of a single property defined by getter and/or setter method
   ''' </summary>
   ''' <typeparam name="TProperty"></typeparam>
   ''' <remarks></remarks>
   Public Class DynamicProperty(Of TProperty)

      Protected Sub New(propertyName As String)
      End Sub
      Public Sub New(getter As Func(Of TProperty), propertyName As String)
         Me.New(getter, Nothing, propertyName)
      End Sub
      Public Sub New(setter As Action(Of TProperty), propertyName As String)
         Me.New(Nothing, setter, propertyName)
      End Sub
      Public Sub New(getter As Func(Of TProperty), setter As Action(Of TProperty), propertyName As String)
         _getter = getter
         _setter = setter
         Me.PropertyName = propertyName
      End Sub

      Private ReadOnly _getter As Func(Of TProperty)

      ''' <summary>
      ''' Returns True if a getter is defined
      ''' </summary>
      Public Overridable ReadOnly Property CanRead As Boolean
         Get
            Return _getter IsNot Nothing
         End Get
      End Property

      ''' <summary>
      ''' Returns True if a setter is defined
      ''' </summary>
      Public Overridable ReadOnly Property CanWrite As Boolean
         Get
            Return _setter IsNot Nothing
         End Get
      End Property

      Private ReadOnly _setter As Action(Of TProperty)
      Public ReadOnly PropertyName As String

      ''' <summary>
      ''' Property-representation of getter and setter methods
      ''' </summary>
      Public Overridable Property Value As TProperty
         Get
            If _getter Is Nothing Then
               Throw New NotImplementedException()
            Else
               Return _getter.Invoke()
            End If
         End Get
         Set(value As TProperty)
            If _setter Is Nothing Then
               Throw New NotImplementedException()
            Else
               _setter.Invoke(value)
            End If
         End Set
      End Property

   End Class

   ''' <summary>
   ''' Encapsulation of a single property with one indexparameter defined by getter and/or setter method
   ''' </summary>
   ''' <typeparam name="TProperty"></typeparam>
   ''' <remarks></remarks>
   Public Class DynamicProperty(Of TArg1, TProperty)

      Public Sub New(getter As Func(Of TArg1, TProperty), propertyName As String)
         Me.New(getter, Nothing, propertyName)
      End Sub
      Public Sub New(setter As Action(Of TArg1, TProperty), propertyName As String)
         Me.New(Nothing, setter, propertyName)
      End Sub
      Public Sub New(getter As Func(Of TArg1, TProperty), setter As Action(Of TArg1, TProperty), propertyName As String)
         _getter = getter
         _setter = setter
         Me.PropertyName = propertyName
      End Sub

      Private ReadOnly _getter As Func(Of TArg1, TProperty)

      ''' <summary>
      ''' Returns True if a getter is defined
      ''' </summary>
      Public ReadOnly Property CanRead As Boolean
         Get
            Return _getter IsNot Nothing
         End Get
      End Property

      ''' <summary>
      ''' Returns True if a setter is defined
      ''' </summary>
      Public ReadOnly Property CanWrite As Boolean
         Get
            Return _setter IsNot Nothing
         End Get
      End Property

      Private ReadOnly _setter As Action(Of TArg1, TProperty)
      Public ReadOnly PropertyName As String

      ''' <summary>
      ''' Property-representation of getter and setter methods
      ''' </summary>
      Public Property Value(arg1 As TArg1) As TProperty
         Get
            If _getter Is Nothing Then
               Throw New NotImplementedException()
            Else
               Return _getter.Invoke(arg1)
            End If
         End Get
         Set(value As TProperty)
            If _setter Is Nothing Then
               Throw New NotImplementedException()
            Else
               _setter.Invoke(arg1, value)
            End If
         End Set
      End Property

   End Class
End Namespace