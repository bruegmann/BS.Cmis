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
Imports ss = System.ServiceModel

Namespace CmisObjectModel.Common
   <HideModuleName()>
   Public Module AppSettings

#Region "Constants"
      Public Const CacheLeaseTimeKey As String = "CmisObjectModel.Cache.LeaseTime"
      Public Const CacheSizeObjectsKey As String = "CmisObjectModel.Cache.Objects.Size"
      Public Const CacheSizeRepositoriesKey As String = "CmisObjectModel.Cache.Repositories.Size"
      Public Const CacheSizeTypesKey As String = "CmisObjectModel.Cache.Types.Size"
      Public Const ClientCredentialTypeKey As String = "CmisObjectModel.ClientCredentialType"
      Public Const CompressionKey As String = "CmisObjectModel.Compression"
      Public Const SupportDebugInformationKey As String = "CmisObjectModel.Debug"
#End Region

#Region "AppSettings"
      Private _cacheLeaseTime As Double? = Nothing
      Public ReadOnly Property CacheLeaseTime As Double
         Get
            If Not _cacheLeaseTime.HasValue Then
               'Default: 10 minutes
               _cacheLeaseTime = ReadValue(Of Double)(CacheLeaseTimeKey, 600.0)
            End If

            Return _cacheLeaseTime.Value
         End Get
      End Property

      Private _cacheSizeObjects As Integer? = Nothing
      Public ReadOnly Property CacheSizeObjects As Integer
         Get
            If Not _cacheSizeObjects.HasValue Then
               'Default: 500
               _cacheSizeObjects = ReadValue(Of Integer)(CacheSizeObjectsKey, 500)
               If _cacheSizeObjects.Value < 1 Then _cacheSizeObjects = 500
            End If

            Return _cacheSizeObjects.Value
         End Get
      End Property

      Private _cacheSizeRepositories As Integer? = Nothing
      Public ReadOnly Property CacheSizeRepositories As Integer
         Get
            If Not _cacheSizeRepositories.HasValue Then
               'Default: 10
               _cacheSizeRepositories = ReadValue(Of Integer)(CacheSizeRepositoriesKey, 10)
               If _cacheSizeRepositories.Value < 1 Then _cacheSizeRepositories = 10
            End If

            Return _cacheSizeRepositories.Value
         End Get
      End Property

      Private _cacheSizeTypes As Integer? = Nothing
      Public ReadOnly Property CacheSizeTypes As Integer
         Get
            If Not _cacheSizeTypes.HasValue Then
               'Default: 100
               _cacheSizeTypes = ReadValue(Of Integer)(CacheSizeTypesKey, 100)
               If _cacheSizeTypes.Value < 1 Then _cacheSizeTypes = 100
            End If

            Return _cacheSizeTypes.Value
         End Get
      End Property

      Private _clientCredentialType As ss.HttpClientCredentialType? = Nothing
      Public ReadOnly Property ClientCredentialType As ss.HttpClientCredentialType
         Get
            If Not _clientCredentialType.HasValue Then
               _clientCredentialType = ReadEnum(ClientCredentialTypeKey, System.ServiceModel.HttpClientCredentialType.Basic)
            End If

            Return _clientCredentialType.Value
         End Get
      End Property

      Private _compression As Boolean? = Nothing
      Public ReadOnly Property Compression As Boolean
         Get
            If Not _compression.HasValue Then
               'False is default!
               _compression = ReadBoolean(CompressionKey, False)
            End If

            Return _compression.Value
         End Get
      End Property

      Private _supportsDebugInformation As Boolean? = Nothing
      ''' <summary>
      ''' Should the service return debug informations?
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property SupportsDebugInformation As Boolean
         Get
            If Not _supportsDebugInformation.HasValue Then
               'True is default!
               _supportsDebugInformation = ReadBoolean(SupportDebugInformationKey, True)
            End If

            Return _supportsDebugInformation.Value
         End Get
      End Property
#End Region

#Region "Read methods via System.Configuration.ConfigurationManager"
      ''' <summary>
      ''' Reads any Boolean from the AppSettings
      ''' </summary>
      ''' <param name="key"></param>
      ''' <param name="defaultValue"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function ReadBoolean(key As String, defaultValue As Boolean) As Boolean
         Dim value As String = System.Configuration.ConfigurationManager.AppSettings(key)
         Dim boolValue As Boolean
         If defaultValue Then
            'True is default!
            Return String.IsNullOrEmpty(value) OrElse Not Boolean.TryParse(value, boolValue) OrElse boolValue
         Else
            'False is default!
            Return Not String.IsNullOrEmpty(value) AndAlso Boolean.TryParse(value, boolValue) AndAlso boolValue
         End If
      End Function

      ''' <summary>
      ''' Reads any Enum from the AppSettings
      ''' </summary>
      ''' <typeparam name="TEnum"></typeparam>
      ''' <param name="key"></param>
      ''' <param name="defaultValue"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function ReadEnum(Of TEnum As Structure)(key As String, defaultValue As TEnum) As TEnum
         Dim value As String = System.Configuration.ConfigurationManager.AppSettings(key)
         Dim result As TEnum

         Return If(Not String.IsNullOrEmpty(value) AndAlso System.Enum.TryParse(value, True, result), result, defaultValue)
      End Function

      ''' <summary>
      ''' Reads specified value-type from the AppSettings
      ''' </summary>
      ''' <typeparam name="TValue"></typeparam>
      ''' <param name="key"></param>
      ''' <param name="defaultValue"></param>
      ''' <returns></returns>
      ''' <remarks>Specified value-type MUST be defined in DefaultXmlConverter</remarks>
      Private Function ReadValue(Of TValue)(key As String, defaultValue As TValue) As TValue
         Try
            Dim value As String = System.Configuration.ConfigurationManager.AppSettings(key)

            Return If(DefaultXmlConverter.ContainsKey(GetType(TValue)),
                      CType(DefaultXmlConverter(GetType(TValue)), Tuple(Of Func(Of TValue, String), Func(Of String, TValue))).Item2(value), defaultValue)
         Catch
            Return defaultValue
         End Try
      End Function
#End Region

   End Module
End Namespace