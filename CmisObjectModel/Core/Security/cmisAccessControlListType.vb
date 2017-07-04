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
Imports sxs = System.Xml.Serialization

Namespace CmisObjectModel.Core.Security
   <sxs.XmlRoot("acl", Namespace:=Constants.Namespaces.cmis)>
   Partial Public Class cmisAccessControlListType

#Region "Helper classes"
      ''' <summary>
      ''' Key of Map-class
      ''' </summary>
      ''' <remarks></remarks>
      Private Class Key
         Public Sub New(principalId As String, direct As Boolean, permission As String)
            Me.PrincipalId = principalId
            Me.Direct = direct
            Me.Permission = permission
         End Sub

         Public ReadOnly Direct As Boolean
         Public ReadOnly Permission As String
         Public ReadOnly PrincipalId As String
      End Class

      ''' <summary>
      ''' Maps the aces of a cmisAccessControlListType-instance to enable quick transformations
      ''' </summary>
      ''' <remarks></remarks>
      Private Class Map
         Inherits Dictionary(Of String, Dictionary(Of Boolean, Dictionary(Of String, Integer)))

         ''' <summary>
         ''' Adds a permission
         ''' </summary>
         ''' <remarks></remarks>
         Public Shadows Sub Add(key As Key)
            Dim innerMap As Dictionary(Of Boolean, Dictionary(Of String, Integer))
            Dim permissions As Dictionary(Of String, Integer)

            If key.PrincipalId IsNot Nothing Then
               If ContainsKey(key.PrincipalId) Then
                  innerMap = Item(key.PrincipalId)
               Else
                  innerMap = New Dictionary(Of Boolean, Dictionary(Of String, Integer))
                  MyBase.Add(key.PrincipalId, innerMap)
               End If

               If innerMap.ContainsKey(key.Direct) Then
                  permissions = innerMap(key.Direct)
               Else
                  permissions = New Dictionary(Of String, Integer)
                  innerMap.Add(key.Direct, permissions)
               End If

               If Not (String.IsNullOrEmpty(key.Permission) OrElse permissions.ContainsKey(key.Permission)) Then permissions.Add(key.Permission, permissions.Count)
            End If
         End Sub

         ''' <summary>
         ''' Returns True, if a permission is defined for a valid principalId
         ''' </summary>
         ''' <returns></returns>
         ''' <remarks>key.Direct-value is ignored</remarks>
         Public Shadows Function Contains(key As Key) As Boolean
            If key.PrincipalId IsNot Nothing AndAlso Not String.IsNullOrEmpty(key.Permission) AndAlso Me.ContainsKey(key.PrincipalId) Then
               With Me.Item(key.PrincipalId)
                  'ignore the key.Direct-value
                  For Each direct As Boolean In New Boolean() {False, True}
                     If .ContainsKey(direct) AndAlso .Item(direct).ContainsKey(key.Permission) Then Return True
                  Next
               End With
            End If

            Return False
         End Function

         ''' <summary>
         ''' Returns all defined principalId, direct, permission tuples
         ''' </summary>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Shadows Function Keys() As List(Of Key)
            Dim retVal As New List(Of Key)

            For Each deMajor As KeyValuePair(Of String, Dictionary(Of Boolean, Dictionary(Of String, Integer))) In Me
               For Each deMinor As KeyValuePair(Of Boolean, Dictionary(Of String, Integer)) In deMajor.Value
                  Dim permissions As KeyValuePair(Of String, Integer)() =
                     (From de As KeyValuePair(Of String, Integer) In deMinor.Value
                      Where de.Value >= 0
                      Select de
                      Order By de.Value).ToArray()
                  If permissions Is Nothing OrElse permissions.Length = 0 Then
                     retVal.Add(New Key(deMajor.Key, deMinor.Key, Nothing))
                  Else
                     For Each permission As KeyValuePair(Of String, Integer) In permissions
                        retVal.Add(New Key(deMajor.Key, deMinor.Key, permission.Key))
                     Next
                  End If
               Next
            Next

            Return retVal
         End Function

         ''' <summary>
         ''' physical remove
         ''' </summary>
         ''' <remarks>key.Direct-value is ignored</remarks>
         Public Shadows Function Remove(key As Key) As Boolean
            Dim retVal As Boolean = False

            If key.PrincipalId IsNot Nothing AndAlso Not String.IsNullOrEmpty(key.Permission) AndAlso ContainsKey(key.PrincipalId) Then
               With Item(key.PrincipalId)
                  'ignore the key.Direct-value
                  For Each direct As Boolean In New Boolean() {False, True}
                     If .ContainsKey(direct) AndAlso .Item(direct).Remove(key.Permission) Then retVal = True
                  Next
               End With
            End If

            Return retVal
         End Function

         ''' <summary>
         ''' Converts instance to a cmisAccessControlListType
         ''' </summary>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Function ToACEs() As cmisAccessControlListType
            Dim aces As New List(Of cmisAccessControlEntryType)

            For Each deMajor As KeyValuePair(Of String, Dictionary(Of Boolean, Dictionary(Of String, Integer))) In Me
               For Each deMinor As KeyValuePair(Of Boolean, Dictionary(Of String, Integer)) In deMajor.Value
                  Dim permissions As KeyValuePair(Of String, Integer)() =
                     (From de As KeyValuePair(Of String, Integer) In deMinor.Value
                      Where de.Value >= 0
                      Select de
                      Order By de.Value).ToArray()
                  aces.Add(New cmisAccessControlEntryType() With {.Direct = deMinor.Key,
                                                                  .Permissions = If(permissions Is Nothing OrElse permissions.Length = 0, Nothing,
                                                                                    (From de As KeyValuePair(Of String, Integer) In permissions
                                                                                     Select de.Key).ToArray()),
                                                                  .Principal = New cmisAccessControlPrincipalType With {.PrincipalId = deMajor.Key}})
               Next
            Next

            Return New cmisAccessControlListType With {._permissions = If(aces.Count = 0, Nothing, aces.ToArray())}
         End Function

         ''' <summary>
         ''' Converts value to a Map-instance
         ''' </summary>
         ''' <param name="value"></param>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Shared Widening Operator CType(value As cmisAccessControlListType) As Map
            If value Is Nothing Then
               Return Nothing
            Else
               Dim retVal As New Map

               If value._permissions IsNot Nothing Then
                  For Each ace As cmisAccessControlEntryType In value._permissions
                     If ace IsNot Nothing AndAlso ace.Principal IsNot Nothing AndAlso ace.Principal.PrincipalId IsNot Nothing Then
                        If ace.Permissions Is Nothing OrElse ace.Permissions.Length = 0 Then
                           retVal.Add(New Key(ace.Principal.PrincipalId, ace.Direct, Nothing))
                        Else
                           For Each permission As String In ace.Permissions
                              retVal.Add(New Key(ace.Principal.PrincipalId, ace.Direct, permission))
                           Next
                        End If
                     End If
                  Next
               End If

               Return retVal
            End If
         End Operator
      End Class
      ''' <summary>
      ''' Class contains the addACEs- and removeACEs-operations to create the targetACEs starting from
      ''' a given cmisAccessControlListType-instance
      ''' </summary>
      ''' <remarks>see cmisAccessControlListType.Split()</remarks>
      Public Class SplitResult
         Public Sub New(addACEs As cmisAccessControlListType, removeACEs As cmisAccessControlListType)
            Me.AddACEs = addACEs
            Me.RemoveACEs = removeACEs
         End Sub

         Public ReadOnly AddACEs As cmisAccessControlListType
         Public ReadOnly RemoveACEs As cmisAccessControlListType
      End Class
#End Region

      ''' <summary>
      ''' Same as property Permissions; using the BrowserBinding the Permissions-parameter is called aces
      ''' </summary>
      Public Property ACEs As cmisAccessControlEntryType()
         Get
            Return _permissions
         End Get
         Set(value As cmisAccessControlEntryType())
            Me.Permissions = value
         End Set
      End Property

      ''' <summary>
      ''' Aspect before ReadXmlCore()
      ''' </summary>
      Protected Overrides Sub BeginReadXmlCore(reader As System.Xml.XmlReader, attributeOverrides As Serialization.XmlAttributeOverrides)
         'unused
      End Sub

      ''' <summary>
      ''' Aspect before WriteXmlCore()
      ''' </summary>
      Protected Overrides Sub BeginWriteXmlCore(writer As System.Xml.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         'unused
      End Sub

      ''' <summary>
      ''' Aspect after ReadXmlCore()
      ''' </summary>
      Protected Overrides Sub EndReadXmlCore(reader As System.Xml.XmlReader, attributeOverrides As Serialization.XmlAttributeOverrides)
         _exact = Read(reader, attributeOverrides, "exact", Constants.Namespaces.cmis, _exact)
      End Sub

      ''' <summary>
      ''' Aspect after WriteXmlCore()
      ''' </summary>
      Protected Overrides Sub EndWriteXmlCore(writer As System.Xml.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         If _exact.HasValue Then WriteElement(writer, attributeOverrides, "exact", Constants.Namespaces.cmis, Convert(_exact))
      End Sub

      Protected _exact As Boolean?
      ''' <summary>
      ''' Property not defined in CMIS-definition (CMIS-Core.xsd), but in chapter 2.2.10.2.2 Outputs is a property exact defined.
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks>
      ''' The sample given in http://docs.oasis-open.org/cmis/CMIS/v1.1/os/examples/atompub/getAcl-response.log shows, that the
      ''' Core.cmisAccessControlListType is used for transmission. A second class definition contains the exact-property
      ''' (Messaging.cmisACLType). Unfortunately the ACL-property is of type cmisAccessControlListType and not defined as an
      ''' array of cmisAccessControlEntry.
      ''' </remarks>
      Public Overridable Property Exact As Boolean?
         Get
            Return _exact
         End Get
         Set(value As Boolean?)
            If Not _exact.Equals(value) Then
               Dim oldValue As Boolean? = _exact
               _exact = value
               OnPropertyChanged("Exact", value, oldValue)
               OnPropertyChanged("IsExact", value, oldValue)
            End If
         End Set
      End Property 'Exact

      ''' <summary>
      ''' Same as property Exact; using the BrowserBinding the Exact-parameter is called IsExact
      ''' </summary>
      Public Property IsExact As Boolean?
         Get
            Return _exact
         End Get
         Set(value As Boolean?)
            Me.Exact = value
         End Set
      End Property 'IsExact

      ''' <summary>
      ''' Adds the aces specified in addACEs and removes the aces specified in removeACEs from the current instance
      ''' </summary>
      ''' <param name="addACEs"></param>
      ''' <param name="removeACEs"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function Join(addACEs As cmisAccessControlListType, removeACEs As cmisAccessControlListType) As cmisAccessControlListType
         With CType(Me, Map)
            If addACEs IsNot Nothing Then
               For Each key As Key In CType(addACEs, Map).Keys
                  .Add(New Key(key.PrincipalId, True, key.Permission))
               Next
            End If
            If removeACEs IsNot Nothing Then
               For Each key As Key In CType(removeACEs, Map).Keys
                  .Remove(key)
               Next
            End If

            Return .ToACEs
         End With
      End Function

      ''' <summary>
      ''' Calculates the addACEs- and removeACEs-operation to transform the current instance into targetACEs
      ''' </summary>
      ''' <param name="targetACEs"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function Split(targetACEs As cmisAccessControlListType) As SplitResult
         If targetACEs Is Nothing OrElse targetACEs._permissions Is Nothing OrElse targetACEs._permissions.Length = 0 Then
            'remove all aces
            Return New SplitResult(Nothing, Me)
         Else
            Dim addACEs As Map = targetACEs
            Dim removeACEs As New Map

            With CType(Me, Map)
               For Each key As Key In .Keys
                  'this ace is not defined within the targetACEs instance
                  If Not addACEs.Remove(key) Then
                     removeACEs.Add(New Key(key.PrincipalId, True, key.Permission))
                  End If
               Next
            End With

            Return New SplitResult(addACEs.ToACEs, removeACEs.ToACEs)
         End If
      End Function

   End Class
End Namespace