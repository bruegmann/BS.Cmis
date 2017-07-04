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
Imports st = System.Text
Imports str = System.Text.RegularExpressions

Namespace CmisObjectModel.ServiceModel.Query
   ''' <summary>
   ''' Base class of all expressions represented in more than one matches in the query string
   ''' </summary>
   ''' <remarks></remarks>
   Public MustInherit Class CompositeExpression
      Inherits Expression

#Region "Constructors"
      Protected Sub New(match As str.Match, groupName As String, rank As Integer, index As Integer,
                        childrenSeparator As String, childBlockSeparator As String)
         MyBase.New(match, groupName, rank, index)
         _childrenSeparator = childrenSeparator
         _childBlockSeparator = childBlockSeparator
      End Sub
#End Region

#Region "Helper classes"
      Public Delegate Function fnContinueWith(instance As CompositeExpression, continueWith As enumContinueWith) As Boolean

      Public Enum enumContinueWith
         currentInstance
         children
      End Enum

      ''' <summary>
      ''' Simple tree of children
      ''' </summary>
      ''' <remarks></remarks>
      Public Class GetDescendantsResult

         Public Sub New(currentInstance As CompositeExpression, matchSelector As Func(Of Expression, Boolean),
                        instanceSelector As Func(Of CompositeExpression, CompositeExpression))
            Me.New(currentInstance, matchSelector, instanceSelector, CType(Nothing, GetDescendantsResult))
         End Sub
         Private Sub New(currentInstance As CompositeExpression, matchSelector As Func(Of Expression, Boolean),
                         instanceSelector As Func(Of CompositeExpression, CompositeExpression),
                         parent As GetDescendantsResult)
            Dim childrenDescendants As New List(Of GetDescendantsResult)
            Dim children As New List(Of Expression)(currentInstance.GetChildren(matchSelector))

            Me.Instance = instanceSelector(currentInstance)
            Me.Parent = parent
            For Each child As Expression In currentInstance._children
               If TypeOf child Is CompositeExpression Then
                  Dim childDescendant As New GetDescendantsResult(CType(child, CompositeExpression), matchSelector, instanceSelector, Me)

                  If childDescendant.Instance Is Me.Instance Then
                     'combine results (via instanceSelector the same instance has been chosen)
                     children.AddRange(childDescendant.Children)
                     childrenDescendants.AddRange(childDescendant.ChildrenDescendants)
                  ElseIf childDescendant.Children.Length > 0 Then
                     'normal
                     childrenDescendants.Add(childDescendant)
                  ElseIf childDescendant.ChildrenDescendants.Length > 0 Then
                     'skip levels without children
                     childrenDescendants.AddRange(childDescendant.ChildrenDescendants)
                  End If
               End If
            Next
            Me.Children = children.ToArray()
            Me.ChildrenDescendants = childrenDescendants.ToArray()
         End Sub

         Public Sub New(currentInstance As CompositeExpression, matchSelector As Func(Of Expression, Boolean),
                        instanceSelector As Func(Of CompositeExpression, CompositeExpression),
                        continueWith As fnContinueWith)
            Me.New(currentInstance, matchSelector, instanceSelector, continueWith, Nothing)
         End Sub
         Private Sub New(currentInstance As CompositeExpression, matchSelector As Func(Of Expression, Boolean),
                         instanceSelector As Func(Of CompositeExpression, CompositeExpression),
                         continueWith As fnContinueWith,
                         parent As GetDescendantsResult)
            Dim childrenDescendants As New List(Of GetDescendantsResult)
            Dim children As New List(Of Expression)(currentInstance.GetChildren(matchSelector))

            Me.Instance = instanceSelector(currentInstance)
            Me.Parent = parent
            If continueWith(currentInstance, enumContinueWith.children) Then
               For Each child As Expression In currentInstance._children
                  If TypeOf child Is CompositeExpression AndAlso
                     continueWith(CType(child, CompositeExpression), enumContinueWith.currentInstance) Then
                     Dim childDescendant As New GetDescendantsResult(CType(child, CompositeExpression), matchSelector, instanceSelector, continueWith, Me)

                     If childDescendant.Instance Is Me.Instance Then
                        'combine results (via instanceSelector the same instance has been chosen)
                        children.AddRange(childDescendant.Children)
                        childrenDescendants.AddRange(childDescendant.ChildrenDescendants)
                     ElseIf childDescendant.Children.Length > 0 Then
                        'normal
                        childrenDescendants.Add(childDescendant)
                     ElseIf childDescendant.ChildrenDescendants.Length > 0 Then
                        'skip levels without children
                        childrenDescendants.AddRange(childDescendant.ChildrenDescendants)
                     End If
                  End If
               Next
            End If
            Me.Children = children.ToArray()
            Me.ChildrenDescendants = childrenDescendants.ToArray()
         End Sub

         Public ReadOnly Children As Expression()
         Public ReadOnly ChildrenDescendants As GetDescendantsResult()
         Public ReadOnly Instance As CompositeExpression
         Public ReadOnly Parent As GetDescendantsResult

         Public Function ToList() As List(Of Expression)
            Dim retVal As New List(Of Expression)
            ToList(retVal)
            Return retVal
         End Function
         Private Sub ToList(list As List(Of Expression))
            list.AddRange(Children)
            For Each subList As GetDescendantsResult In ChildrenDescendants
               subList.ToList(list)
            Next
         End Sub
      End Class

      ''' <summary>
      ''' Simple generic tree of children
      ''' </summary>
      ''' <remarks></remarks>
      Public Class GetDescendantsResult(Of TInstance As CompositeExpression, TChild As Expression)

         Public Sub New(currentInstance As CompositeExpression, matchSelector As Func(Of TChild, Boolean),
                        instanceSelector As Func(Of CompositeExpression, TInstance))
            Me.New(currentInstance, matchSelector, instanceSelector,
                   CType(Nothing, GetDescendantsResult(Of TInstance, TChild)))
         End Sub
         Private Sub New(currentInstance As CompositeExpression, matchSelector As Func(Of TChild, Boolean),
                         instanceSelector As Func(Of CompositeExpression, TInstance),
                         parent As GetDescendantsResult(Of TInstance, TChild))
            Dim childrenDescendants As New List(Of GetDescendantsResult(Of TInstance, TChild))
            Dim children As New List(Of TChild)(currentInstance.GetChildren(matchSelector))

            Me.Instance = instanceSelector(currentInstance)
            Me.Parent = parent
            For Each child As Expression In currentInstance._children
               If TypeOf child Is CompositeExpression Then
                  Dim childDescendant As New GetDescendantsResult(Of TInstance, TChild)(CType(child, CompositeExpression), matchSelector, instanceSelector, Me)

                  If childDescendant.Instance Is Me.Instance Then
                     'combine results (via instanceSelector the same instance has been chosen)
                     children.AddRange(childDescendant.Children)
                     childrenDescendants.AddRange(childDescendant.ChildrenDescendants)
                  ElseIf childDescendant.Children.Length > 0 Then
                     'normal
                     childrenDescendants.Add(childDescendant)
                  ElseIf childDescendant.ChildrenDescendants.Length > 0 Then
                     'skip levels without children
                     childrenDescendants.AddRange(childDescendant.ChildrenDescendants)
                  End If
               End If
            Next
            Me.Children = children.ToArray()
            Me.ChildrenDescendants = childrenDescendants.ToArray()
         End Sub

         Public Sub New(currentInstance As CompositeExpression, matchSelector As Func(Of TChild, Boolean),
                        instanceSelector As Func(Of CompositeExpression, TInstance),
                        continueWith As fnContinueWith)
            Me.New(currentInstance, matchSelector, instanceSelector, continueWith, Nothing)
         End Sub
         Private Sub New(currentInstance As CompositeExpression, matchSelector As Func(Of TChild, Boolean),
                         instanceSelector As Func(Of CompositeExpression, TInstance),
                         continueWith As fnContinueWith,
                         parent As GetDescendantsResult(Of TInstance, TChild))
            Dim childrenDescendants As New List(Of GetDescendantsResult(Of TInstance, TChild))
            Dim children As New List(Of TChild)(currentInstance.GetChildren(matchSelector))

            Me.Instance = instanceSelector(currentInstance)
            Me.Parent = parent
            If continueWith(currentInstance, enumContinueWith.children) Then
               For Each child As Expression In currentInstance._children
                  If TypeOf child Is CompositeExpression AndAlso
                     continueWith(CType(child, CompositeExpression), enumContinueWith.currentInstance) Then
                     Dim childDescendant As New GetDescendantsResult(Of TInstance, TChild)(CType(child, CompositeExpression), matchSelector, instanceSelector, Me)

                     If childDescendant.Instance Is Me.Instance Then
                        'combine results (via instanceSelector the same instance has been chosen)
                        children.AddRange(childDescendant.Children)
                        childrenDescendants.AddRange(childDescendant.ChildrenDescendants)
                     ElseIf childDescendant.Children.Length > 0 Then
                        'normal
                        childrenDescendants.Add(childDescendant)
                     ElseIf childDescendant.ChildrenDescendants.Length > 0 Then
                        'skip levels without children
                        childrenDescendants.AddRange(childDescendant.ChildrenDescendants)
                     End If
                  End If
               Next
            End If
            Me.Children = children.ToArray()
            Me.ChildrenDescendants = childrenDescendants.ToArray()
         End Sub

         Public ReadOnly Children As TChild()
         Public ReadOnly ChildrenDescendants As GetDescendantsResult(Of TInstance, TChild)()
         Public ReadOnly Instance As TInstance
         Public ReadOnly Parent As GetDescendantsResult(Of TInstance, TChild)

         Public Function ToList() As List(Of TChild)
            Dim retVal As New List(Of TChild)
            ToList(retVal)
            Return retVal
         End Function
         Private Sub ToList(list As List(Of TChild))
            list.AddRange(Children)
            For Each subList As GetDescendantsResult(Of TInstance, TChild) In ChildrenDescendants
               subList.ToList(list)
            Next
         End Sub
      End Class
#End Region

      Protected _childBlockSeparator As String
      Protected _children As New List(Of Expression)
      Protected _childrenSeparator As String

      ''' <summary>
      ''' Returns the count of children
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property Count As Integer
         Get
            Return _children.Count
         End Get
      End Property

      ''' <summary>
      ''' Returns the identifiers of fields contained within the current instance-type
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property FieldIdentifiers As GetDescendantsResult
         Get
            Return GetIdentifiers(Of FieldExpression)()
         End Get
      End Property

      ''' <summary>
      ''' Returns a list of matching children
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetChildren(matchSelector As Func(Of Expression, Boolean)) As Expression()
         Return (From child As Expression In _children
                 Where matchSelector(child)
                 Select child).ToArray()
      End Function
      ''' <summary>
      ''' Returns a list of matching children
      ''' </summary>
      Public Function GetChildren(Of TExpression As Expression)() As TExpression()
         Return (From child As Expression In _children
                 Where TypeOf child Is TExpression
                 Select CType(child, TExpression)).ToArray()
      End Function
      ''' <summary>
      ''' Returns a list of matching children
      ''' </summary>
      Public Function GetChildren(Of TExpression As Expression)(matchSelector As Func(Of TExpression, Boolean)) As TExpression()
         Return (From child As Expression In _children
                 Where TypeOf child Is TExpression AndAlso matchSelector(CType(child, TExpression))
                 Select CType(child, TExpression)).ToArray()
      End Function

      ''' <summary>
      ''' Returns a tree of matching descendants
      ''' </summary>
      ''' <param name="matchSelector">Decision which child should be returned</param>
      ''' <param name="instanceSelector">If set the returned value is set for GetDescendantsResult.Instance property.
      ''' If not set the GetDescendantsResult.Instance property is the parent not of all items listet in
      ''' GetDescendantsResult.Children property.</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetDescendants(matchSelector As Func(Of Expression, Boolean),
                                     Optional instanceSelector As Func(Of CompositeExpression, CompositeExpression) = Nothing) As GetDescendantsResult
         Return New GetDescendantsResult(Me, matchSelector, If(instanceSelector, Function(instance) instance))
      End Function

      ''' <summary>
      ''' Returns a tree of matching descendants
      ''' </summary>
      ''' <param name="continueWith">Callback to decide when stop building the tree</param>
      ''' <param name="matchSelector">Decision which child should be returned</param>
      ''' <param name="instanceSelector">If set the returned value is set for GetDescendantsResult.Instance property.
      ''' If not set the GetDescendantsResult.Instance property is the parent not of all items listet in
      ''' GetDescendantsResult.Children property.</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetDescendants(continueWith As fnContinueWith, matchSelector As Func(Of Expression, Boolean),
                                     Optional instanceSelector As Func(Of CompositeExpression, CompositeExpression) = Nothing) As GetDescendantsResult
         Return New GetDescendantsResult(Me, matchSelector, If(instanceSelector, Function(instance) instance), continueWith)
      End Function

      ''' <summary>
      ''' Returns a tree of matching descendants
      ''' </summary>
      ''' <param name="matchSelector">Decision which child should be returned</param>
      ''' <param name="instanceSelector">If set the returned value is set for GetDescendantsResult.Instance property.
      ''' If not set the GetDescendantsResult.Instance property is the parent not of all items listet in
      ''' GetDescendantsResult.Children property.</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetDescendants(Of TInstance As CompositeExpression, TChild As Expression)(matchSelector As Func(Of TChild, Boolean),
                                                                                                Optional instanceSelector As Func(Of CompositeExpression, TInstance) = Nothing) As GetDescendantsResult(Of TInstance, TChild)
         Return New GetDescendantsResult(Of TInstance, TChild)(Me, matchSelector,
                                                               If(instanceSelector,
                                                                  New Func(Of CompositeExpression, TInstance)(Function(instance)
                                                                                                                 If TypeOf instance Is TInstance Then
                                                                                                                    Return CType(instance, TInstance)
                                                                                                                 Else
                                                                                                                    Return instance.GetAncestor(Of TInstance)()
                                                                                                                 End If
                                                                                                              End Function)))
      End Function

      ''' <summary>
      ''' Returns a tree of matching descendants
      ''' </summary>
      ''' <param name="continueWith">Callback to decide when stop building the tree</param>
      ''' <param name="matchSelector">Decision which child should be returned</param>
      ''' <param name="instanceSelector">If set the returned value is set for GetDescendantsResult.Instance property.
      ''' If not set the GetDescendantsResult.Instance property is the parent not of all items listet in
      ''' GetDescendantsResult.Children property.</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetDescendants(Of TInstance As CompositeExpression, TChild As Expression)(continueWith As fnContinueWith, matchSelector As Func(Of TChild, Boolean),
                                                                                                Optional instanceSelector As Func(Of CompositeExpression, TInstance) = Nothing) As GetDescendantsResult(Of TInstance, TChild)
         Return New GetDescendantsResult(Of TInstance, TChild)(Me, matchSelector,
                                                               If(instanceSelector,
                                                                  New Func(Of CompositeExpression, TInstance)(Function(instance)
                                                                                                                 If TypeOf instance Is TInstance Then
                                                                                                                    Return CType(instance, TInstance)
                                                                                                                 Else
                                                                                                                    Return instance.GetAncestor(Of TInstance)()
                                                                                                                 End If
                                                                                                              End Function)),
                                                               continueWith)
      End Function

      ''' <summary>
      ''' Returns the identifiers of specified DatabaseObjectExpression-type within the current instance-type
      ''' </summary>
      ''' <typeparam name="T"></typeparam>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetIdentifiers(Of T As DatabaseObjectExpression)() As GetDescendantsResult
         Return GetDescendants(Function(expression)
                                  Return TypeOf expression Is IdentifierExpression AndAlso TypeOf expression.GetAncestor(Of DatabaseObjectExpression)() Is T
                               End Function,
                               Function(expression)
                                  Return If(Me.GetType().IsAssignableFrom(expression.GetType()), expression, expression.GetAncestor(Me.GetType()))
                               End Function)
      End Function

      ''' <summary>
      ''' Default implementation of GetValue() supports a children block separated by _childrenSeparator embedded in
      ''' the value of the current instance followed by _childBlockSeparator as prefix and _childBlockSeparator
      ''' followed by _termination.ToString() as suffix. The suffix is only present if _termination is not null.
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overrides Function GetValue(executingType As System.Type) As String
         Dim myBaseResult As String = MyBase.GetValue(executingType)

         If Me.GetType().IsAssignableFrom(executingType) Then
            Dim sb As New st.StringBuilder(myBaseResult)

            If _children.Count > 0 Then
               If Not String.IsNullOrEmpty(_childBlockSeparator) AndAlso sb.Length > 0 Then sb.Append(_childBlockSeparator)
               sb.Append(String.Join(If(_childrenSeparator, ""),
                                     (From child As Expression In _children
                                      Let childExpression As String = child.Value
                                      Select childExpression).ToArray()))
            End If
            If _termination IsNot Nothing Then
               If Not String.IsNullOrEmpty(_childBlockSeparator) AndAlso sb.Length > 0 Then sb.Append(_childBlockSeparator)
               sb.Append(_termination.Value)
            End If

            Return sb.ToString()
         Else
            Return myBaseResult
         End If
      End Function

      ''' <summary>
      ''' Replaces a child; the method returns true, if the replacement was successful
      ''' </summary>
      Public Overridable Function ReplaceChild(oldChild As Expression, newChild As Expression) As Boolean
         For index As Integer = 0 To _children.Count - 1
            Dim child As Expression = _children(index)

            If oldChild Is child Then
               If newChild Is Nothing Then
                  _children.RemoveAt(index)
               Else
                  _children(index) = newChild
                  SetParent(newChild, Me)
               End If
               SetParent(oldChild, Nothing)

               Return True
            End If
         Next

         Return False
      End Function

      ''' <summary>
      ''' Returns the identifiers of tables contained within the current instance-type
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property TableIdentifiers As GetDescendantsResult
         Get
            Return GetIdentifiers(Of TableExpression)()
         End Get
      End Property

      Protected _termination As Expression

   End Class
End Namespace