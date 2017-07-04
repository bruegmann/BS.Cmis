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
Imports CmisObjectModel.Constants
Imports ca = CmisObjectModel.AtomPub
Imports ss = System.ServiceModel
Imports sss = System.ServiceModel.Syndication
Imports ssw = System.ServiceModel.Web
Imports sx = System.Xml

Namespace CmisObjectModel.Data
   ''' <summary>
   ''' A simple converter to adapt values from current system to remote system and vice versa
   ''' </summary>
   ''' <remarks></remarks>
   Public Class PropertyValueConverter

      Protected _map As Hashtable
      Protected _mapReverse As Hashtable
      Protected _nullValueMapping As Common.Generic.Nullable(Of Object)
      Protected _nullValueReverseMapping As Common.Generic.Nullable(Of Object)

#Region "Constructors"
      Protected Sub New(localType As Type, remoteType As Type)
         Me.LocalType = localType
         Me.RemoteType = remoteType
      End Sub
      ''' <summary>
      ''' </summary>
      ''' <param name="map">keys: local system objects, values: remote system objects</param>
      ''' <param name="nullValueMapping"></param>
      ''' <remarks></remarks>
      Public Sub New(map As Hashtable, Optional nullValueMapping As Common.Generic.Nullable(Of Object) = Nothing)
         Me.New(map, GetType(Object), GetType(Object), nullValueMapping)
      End Sub
      Public Sub New(map As Hashtable, localType As Type, remoteType As Type, Optional nullValueMapping As Common.Generic.Nullable(Of Object) = Nothing)
         Me.New(localType, remoteType)
         _map = map
         _nullValueMapping = nullValueMapping
         If map IsNot Nothing Then
            _mapReverse = New Hashtable
            If nullValueMapping.Value Is Nothing Then
               _nullValueReverseMapping = nullValueMapping
            Else
               _mapReverse.Add(nullValueMapping.Value, Nothing)
            End If
            For Each de As DictionaryEntry In _map
               If de.Value IsNot Nothing Then
                  If Not _mapReverse.ContainsKey(de.Value) Then _mapReverse.Add(de.Value, de.Key)
               ElseIf Not _nullValueReverseMapping.HasValue Then
                  _nullValueReverseMapping = New Common.Generic.Nullable(Of Object)(de.Key)
               End If
            Next
         End If
      End Sub
#End Region

#Region "Helper classes"
      ''' <summary>
      ''' Skips localType and remoteType of a propertyValueConverter
      ''' </summary>
      ''' <remarks></remarks>
      Private Class InversPropertyValueConverter
         Inherits PropertyValueConverter

         Private _innerConverter As PropertyValueConverter
         Public Sub New(innerConverter As PropertyValueConverter)
            MyBase.New(innerConverter.RemoteType, innerConverter.LocalType)
            _innerConverter = innerConverter
         End Sub

         Public Overrides Function Convert(value As Object) As Object
            Return _innerConverter.ConvertBack(value)
         End Function

         Public Overrides Function ConvertBack(value As Object) As Object
            Return _innerConverter.Convert(value)
         End Function

         Public Overrides Function MakeReverse() As PropertyValueConverter
            Return _innerConverter
         End Function
      End Class
#End Region

      ''' <summary>
      ''' Converts a value-array from the remote system (server/client) to the local system (client/server)
      ''' </summary>
      ''' <param name="values"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function Convert(ParamArray values As Object()) As Object()
         If values Is Nothing Then
            Return Nothing
         Else
            Return (From value As Object In values
                    Select Convert(value)).ToArray()
         End If
      End Function
      ''' <summary>
      ''' Converts a value from the remote system (server/client) to the local system (client/server)
      ''' </summary>
      ''' <param name="value"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overridable Function Convert(value As Object) As Object
         If _mapReverse Is Nothing Then
            Return value
         Else
            Return If(value Is Nothing, _nullValueReverseMapping.Value,
                      If(_mapReverse.ContainsKey(value), _mapReverse(value), value))
         End If
      End Function

      ''' <summary>
      ''' Converts back a value-array from the local system (client/server) to the remote system (server/client)
      ''' </summary>
      ''' <param name="values"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function ConvertBack(ParamArray values As Object()) As Object()
         If values Is Nothing Then
            Return Nothing
         Else
            Return (From value As Object In values
                    Select ConvertBack(value)).ToArray()
         End If
      End Function
      ''' <summary>
      ''' Converts back a value from the local system (client/server) to the remote system (server/client)
      ''' </summary>
      ''' <param name="value"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overridable Function ConvertBack(value As Object) As Object
         If _map IsNot Nothing Then
            Return If(value Is Nothing, _nullValueMapping.Value,
                      If(_map.ContainsKey(value), _map(value), value))
         ElseIf value Is Nothing Then
            Return _nullValueMapping.Value
         Else
            Return value
         End If
      End Function

      Public Overridable Function MakeReverse() As PropertyValueConverter
         Static retVal As New InversPropertyValueConverter(Me)
         Return retVal
      End Function

      Public ReadOnly LocalType As Type
      Public ReadOnly RemoteType As Type

   End Class

   Namespace Generic
      ''' <summary>
      ''' Provides convert method from TValue to TResult
      ''' </summary>
      ''' <typeparam name="TValue"></typeparam>
      ''' <typeparam name="TResult"></typeparam>
      ''' <remarks></remarks>
      Friend Class ConvertDynamicHelper(Of TValue, TResult)

         Private Shared _converter As Func(Of TValue, TResult)
         Private Shared _syncObject As New Object

         Public Shared Function ConvertDynamic(value As TValue) As TResult
            With If(_converter, GetConverter())
               Return .Invoke(value)
            End With
         End Function

         ''' <summary>
         ''' Returns a valid converter from TValue to TResult
         ''' </summary>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Shared Function GetConverter() As Func(Of TValue, TResult)
            SyncLock _syncObject
               'search only once to generate a lambda
               If _converter Is Nothing Then
                  Dim type As Type = GetType(TResult)
                  Dim converters As New List(Of Func(Of TValue, TResult))

                  '1. chance: TypeConverter (ConvertFrom)
                  For Each attr As Attribute In type.GetCustomAttributes(True)
                     If TypeOf attr Is System.ComponentModel.TypeConverterAttribute Then
                        Dim tca As System.ComponentModel.TypeConverterAttribute = CType(attr, System.ComponentModel.TypeConverterAttribute)
                        Dim converterType As Type = System.Type.GetType(tca.ConverterTypeName, False, True)

                        If converterType IsNot Nothing Then
                           Dim converter As System.ComponentModel.TypeConverter = CType(Activator.CreateInstance(converterType), System.ComponentModel.TypeConverter)
                           If converter IsNot Nothing AndAlso converter.CanConvertFrom(GetType(TValue)) Then
                              converters.Add(Function(value) CType(converter.ConvertFrom(value), TResult))
                           End If
                        End If
                     End If
                  Next

                  '2. chance: TypeConverter (ConvertTo)
                  For Each attr As Attribute In GetType(TValue).GetCustomAttributes(True)
                     If TypeOf attr Is System.ComponentModel.TypeConverterAttribute Then
                        Dim tca As System.ComponentModel.TypeConverterAttribute = CType(attr, System.ComponentModel.TypeConverterAttribute)
                        Dim converterType As Type = System.Type.GetType(tca.ConverterTypeName, False, True)

                        If converterType IsNot Nothing Then
                           Dim converter As System.ComponentModel.TypeConverter = CType(Activator.CreateInstance(converterType), System.ComponentModel.TypeConverter)
                           If converter IsNot Nothing AndAlso converter.CanConvertTo(type) Then
                              converters.Add(Function(value) CType(converter.ConvertTo(value, type), TResult))
                           End If
                        End If
                     End If
                  Next

                  '3. chance: direkt
                  If type.IsEnum Then
                     converters.Add(Function(value)
                                       Dim enumValue As String = CStr(CType(value, Object))
                                       Return CType(System.Enum.Parse(type, enumValue, True), TResult)
                                    End Function)
                  Else
                     converters.Add(Function(value) CTypeDynamic(Of TResult)(value))
                  End If

                  'at least there is one converter defined
                  If converters.Count = 1 Then
                     Dim converter As Func(Of TValue, TResult) = converters(0)

                     _converter = Function(value)
                                     Try
                                        Return converter(value)
                                     Catch
                                        Return Nothing
                                     End Try
                                  End Function
                  Else
                     _converter = Function(value)
                                     For Each converter As Func(Of TValue, TResult) In converters
                                        Try
                                           Return converter(value)
                                        Catch
                                        End Try
                                     Next

                                     'not to convert
                                     Return Nothing
                                  End Function
                  End If
               End If

               Return _converter
            End SyncLock
         End Function
      End Class

      ''' <summary>
      ''' A simple typesafe converter to adapt values from remote system to local system and vice versa
      ''' </summary>
      ''' <typeparam name="TValue"></typeparam>
      ''' <remarks></remarks>
      Public Class PropertyValueConverter(Of TValue)
         Inherits PropertyValueConverter(Of TValue, TValue)

         ''' <summary>
         ''' </summary>
         ''' <param name="map">keys: local system objects, values: remote system objects</param>
         ''' <param name="nullValueMapping"></param>
         ''' <remarks></remarks>
         Public Sub New(map As Dictionary(Of TValue, TValue), Optional nullValueMapping As Common.Generic.Nullable(Of TValue) = Nothing)
            MyBase.New(map, nullValueMapping)
         End Sub

         Protected Overrides Function ConvertBackDynamic(value As TValue) As TValue
            Return value
         End Function

         Protected Overrides Function ConvertDynamic(value As TValue) As TValue
            Return value
         End Function
      End Class

      ''' <summary>
      ''' A simple typesafe converter to adapt values from remote system to local system and vice versa
      ''' </summary>
      ''' <typeparam name="TLocal"></typeparam>
      ''' <typeparam name="TRemote"></typeparam>
      ''' <remarks></remarks>
      Public Class PropertyValueConverter(Of TLocal, TRemote)
         Inherits PropertyValueConverter

         Protected Shadows _map As Dictionary(Of TLocal, TRemote)
         Protected Shadows _mapReverse As Dictionary(Of TRemote, TLocal)
         Protected Shadows _nullValueMapping As Common.Generic.Nullable(Of TRemote)
         Protected Shadows _nullValueReverseMapping As Common.Generic.Nullable(Of TLocal)

#Region "Constructors"
         Private Sub New()
            MyBase.New(GetType(TLocal), GetType(TRemote))
         End Sub
         ''' <summary>
         ''' </summary>
         ''' <param name="map">keys: local system objects, values: remote system objects</param>
         ''' <param name="nullValueMapping"></param>
         ''' <remarks></remarks>
         Public Sub New(map As Dictionary(Of TLocal, TRemote), Optional nullValueMapping As Common.Generic.Nullable(Of TRemote) = Nothing)
            Me.New()

            _map = map
            _nullValueMapping = nullValueMapping
            If map IsNot Nothing Then
               _mapReverse = New Dictionary(Of TRemote, TLocal)
               If nullValueMapping.HasValue Then
                  If nullValueMapping.Value Is Nothing Then
                     _nullValueReverseMapping = New Common.Generic.Nullable(Of TLocal)(Nothing)
                  Else
                     _mapReverse.Add(nullValueMapping.Value, Nothing)
                  End If
               End If

               For Each de As KeyValuePair(Of TLocal, TRemote) In _map
                  If de.Value IsNot Nothing Then
                     If Not _mapReverse.ContainsKey(de.Value) Then _mapReverse.Add(de.Value, de.Key)
                  ElseIf Not _nullValueReverseMapping.HasValue Then
                     _nullValueReverseMapping = New Common.Generic.Nullable(Of TLocal)(de.Key)
                  End If
               Next
            End If
         End Sub
#End Region

#Region "Helper classes"
         ''' <summary>
         ''' Skips localType and remoteType of a propertyValueConverter
         ''' </summary>
         ''' <remarks></remarks>
         Private Class InversPropertyValueConverter
            Inherits PropertyValueConverter(Of TRemote, TLocal)

            Private _innerPropertyValueConverter As PropertyValueConverter(Of TLocal, TRemote)
            Public Sub New(innerPropertyValueConverter As PropertyValueConverter(Of TLocal, TRemote))
               _innerPropertyValueConverter = innerPropertyValueConverter
            End Sub

            Public Overrides Function Convert(value As TLocal) As TRemote
               Return _innerPropertyValueConverter.ConvertBack(value)
            End Function

            Public Overrides Function ConvertBack(value As TRemote) As TLocal
               Return _innerPropertyValueConverter.Convert(value)
            End Function

            Public Overrides Function MakeReverse() As PropertyValueConverter
               Return _innerPropertyValueConverter
            End Function
         End Class
#End Region

         ''' <summary>
         ''' Converts a value-array from the remote system (server/client) to the local system (client/server)
         ''' </summary>
         ''' <param name="values"></param>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Overloads Function Convert(ParamArray values As TRemote()) As TLocal()
            If values Is Nothing Then
               Return Nothing
            Else
               Return (From value As TRemote In values
                       Select Convert(value)).ToArray()
            End If
         End Function
         ''' <summary>
         ''' Converts a value from the remote system (server/client) to the local system (client/server)
         ''' </summary>
         ''' <param name="value"></param>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public NotOverridable Overrides Function Convert(value As Object) As Object
            Return If(TypeOf value Is TRemote, Convert(CType(value, TRemote)), value)
         End Function
         ''' <summary>
         ''' Converts a value from the remote system (server/client) to the local system (client/server)
         ''' </summary>
         ''' <param name="value"></param>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Overridable Overloads Function Convert(value As TRemote) As TLocal
            If value Is Nothing Then
               Return _nullValueReverseMapping.Value
            ElseIf _mapReverse Is Nothing OrElse Not _mapReverse.ContainsKey(value) Then
               Return ConvertDynamic(value)
            Else
               Return _mapReverse(value)
            End If
         End Function

         ''' <summary>
         ''' Converts back a value-array from the local system (client/server) to the remote system (server/client)
         ''' </summary>
         ''' <param name="values"></param>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Overloads Function ConvertBack(ParamArray values As TLocal()) As TRemote()
            If values Is Nothing Then
               Return Nothing
            Else
               Return (From value As TLocal In values
                       Select ConvertBack(value)).ToArray()
            End If
         End Function
         ''' <summary>
         ''' Converts back a value from the local system (client/server) to the remote system (server/client)
         ''' </summary>
         ''' <param name="value"></param>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public NotOverridable Overrides Function ConvertBack(value As Object) As Object
            Return If(TypeOf value Is TLocal, ConvertBack(CType(value, TLocal)), value)
         End Function
         ''' <summary>
         ''' Converts back a value from the local system (client/server) to the remote system (server/client)
         ''' </summary>
         ''' <param name="value"></param>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Overridable Overloads Function ConvertBack(value As TLocal) As TRemote
            If value Is Nothing Then
               Return _nullValueMapping.Value
            ElseIf _map Is Nothing OrElse Not _map.ContainsKey(value) Then
               Return ConvertBackDynamic(value)
            Else
               Return _map(value)
            End If
         End Function

         Protected Overridable Function ConvertBackDynamic(value As TLocal) As TRemote
            Return ConvertDynamic(Of TLocal, TRemote)(value)
         End Function
         Protected Overridable Function ConvertDynamic(value As TRemote) As TLocal
            Return ConvertDynamic(Of TRemote, TLocal)(value)
         End Function
         Private Function ConvertDynamic(Of TValue, TResult)(value As TValue) As TResult
            Return ConvertDynamicHelper(Of TValue, TResult).ConvertDynamic(value)
         End Function

         Public Overrides Function MakeReverse() As PropertyValueConverter
            Static retVal As New InversPropertyValueConverter(Me)
            Return retVal
         End Function

      End Class
   End Namespace
End Namespace