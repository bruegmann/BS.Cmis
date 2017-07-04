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
Imports ccg = CmisObjectModel.Common.Generic
Imports str = System.Text.RegularExpressions

Namespace CmisObjectModel.ServiceModel.Query
   ''' <summary>
   ''' Base class for a match in a query string
   ''' </summary>
   ''' <remarks></remarks>
   Public Class Expression

#Region "Constants"
      Public Const NotSetValue As String = "499280f8f2e94625bf5abd29fd2fcc56"
#End Region

#Region "Constructors"
      Public Sub New(match As str.Match, groupName As String, rank As Integer, index As Integer)
         Me.Match = match
         Me.GroupName = groupName
         Me.Index = index
         Me.Rank = rank
      End Sub
#End Region

      ''' <summary>
      ''' Returns True if the Value-property can be set to a custom defined value, otherwise False
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overridable Function CanSetValue() As Boolean
         Return True
      End Function

      ''' <summary>
      ''' Searches for an ancestor of TParent-type
      ''' </summary>
      ''' <typeparam name="TParent"></typeparam>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetAncestor(Of TParent As CompositeExpression)() As TParent
         Dim parent As CompositeExpression = Me.Parent

         While parent IsNot Nothing
            If TypeOf parent Is TParent Then
               Return CType(CObj(parent), TParent)
            Else
               parent = parent.Parent
            End If
         End While

         Return Nothing
      End Function
      ''' <summary>
      ''' Searches for an ancestor of a type defined in ancestorTypes
      ''' </summary>
      ''' <param name="ancestorTypes"></param>
      ''' <returns></returns>
      ''' <remarks>Respects the inheritance of types</remarks>
      Public Function GetAncestor(ParamArray ancestorTypes As Type()) As CompositeExpression
         If ancestorTypes Is Nothing Then
            Return Parent
         Else
            Dim parent As CompositeExpression = Me.Parent

            While parent IsNot Nothing
               Dim parentType As Type = parent.GetType

               For Each ancestorType As Type In ancestorTypes
                  If ancestorType IsNot Nothing AndAlso ancestorType.IsAssignableFrom(parentType) Then
                     Return parent
                  End If
               Next
               parent = parent.Parent
            End While
         End If

         Return Nothing
      End Function

      ''' <summary>
      ''' Returns the next expression on the left side which doesn't belong to the same root
      ''' </summary>
      ''' <param name="expressions"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Friend Function GetLeftExpression(expressions As List(Of Expression)) As Expression
         Dim root = Me.Root

         For index As Integer = index - 1 To 0 Step -1
            Dim retVal = expressions(index)
            If retVal.Root IsNot root Then Return retVal
         Next

         Return Nothing
      End Function

      ''' <summary>
      ''' Returns the next expression on the right side which doesn't belong to the same root
      ''' </summary>
      ''' <param name="expressions"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Friend Function GetRightExpression(expressions As List(Of Expression)) As Expression
         Dim root = Me.Root

         For index As Integer = Me.Index + 1 To expressions.Count - 1
            Dim retVal = expressions(index)
            If retVal.Root IsNot root Then Return retVal
         Next

         Return Nothing
      End Function

      ''' <summary>
      ''' Default implementation returns Match.Value
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overridable Function GetValue(executingType As Type) As String
         Return Match.Value
      End Function

      Public ReadOnly GroupName As String
      Public ReadOnly Index As Integer
      Public ReadOnly Match As str.Match

      Protected _parent As CompositeExpression
      Public ReadOnly Property Parent As CompositeExpression
         Get
            Return _parent
         End Get
      End Property
      Protected Sub SetParent(parent As CompositeExpression)
         If _parent IsNot parent Then
            _parent = parent
            RaiseEvent ParentChanged(Me, EventArgs.Empty)
         End If
      End Sub
      Protected Shared Sub SetParent(instance As Expression, parent As CompositeExpression)
         If instance IsNot Nothing Then instance.SetParent(parent)
      End Sub
      Public Event ParentChanged As EventHandler

      Public ReadOnly Rank As Integer

      ''' <summary>
      ''' Returns the top level expression the current instance belongs to
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property Root As Expression
         Get
            Return If(_parent Is Nothing, Me, _parent.Root)
         End Get
      End Property

      Protected _sealResult As Integer?
      ''' <summary>
      ''' Returns null if the expression is accepted in the parsed query, otherwise the position of the match
      ''' </summary>
      ''' <param name="expressions"></param>
      ''' <returns>Null: success, otherwise position of unexpected expression</returns>
      ''' <remarks></remarks>
      Public Overridable Function Seal(expressions As List(Of Expression)) As Integer?
         _sealed = True
         Return _sealResult
      End Function

      Protected _sealed As Boolean = False
      Public ReadOnly Property Sealed As Boolean
         Get
            Return _sealed
         End Get
      End Property

      Public NotOverridable Overrides Function ToString() As String
         Return Value
      End Function

      Private _value As ccg.Nullable(Of String)
      Public Property Value As String
         Get
            Return If(_value.HasValue, _value.Value, GetValue(Me.GetType()))
         End Get
         Set(value As String)
            If CanSetValue() Then
               If value = NotSetValue Then
                  _value = Nothing
               Else
                  _value = value
               End If
            End If
         End Set
      End Property

   End Class
End Namespace