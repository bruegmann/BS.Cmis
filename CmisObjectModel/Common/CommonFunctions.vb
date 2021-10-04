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
Imports CmisObjectModel.Constants
Imports src = System.Runtime.CompilerServices
Imports srs = System.Runtime.Serialization
Imports ssw = System.ServiceModel.Web
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Common
   ''' <summary>
   ''' a set of common functions
   ''' </summary>
   ''' <remarks></remarks>
   <HideModuleName()>
   Public Module CommonFunctions

      Sub New()
         Try
            Dim defaultLogFile As String = System.Configuration.ConfigurationManager.AppSettings("LogFile")

            If Not String.IsNullOrEmpty(defaultLogFile) AndAlso
               EnsureDirectory(IO.Path.GetDirectoryName(defaultLogFile)) Then
               If Not IO.File.Exists(defaultLogFile) Then
                  Try
                     IO.File.Create(defaultLogFile).Close()
                     _defaultLogFile = defaultLogFile
                  Catch
                  End Try
               Else
                  _defaultLogFile = defaultLogFile
               End If
            End If
         Catch ex As Exception
         End Try

         _defaultNamespacePrefixes = New Dictionary(Of String, String)
         For Each de As KeyValuePair(Of sx.XmlQualifiedName, String) In _namespaces
            Dim key As String = de.Value.ToLowerInvariant()

            If Not _defaultNamespacePrefixes.ContainsKey(key) Then
               _defaultNamespacePrefixes.Add(key, de.Key.Name)
            End If
         Next
      End Sub

#Region "Helper classes"
      ''' <summary>
      ''' Allows a quick map between the name and the value of enum-members
      ''' </summary>
      ''' <remarks></remarks>
      Private Class EnumInspector

         Private Sub New(enumType As Type)
            Dim names As New Dictionary(Of System.Enum, String)
            Dim values As New Dictionary(Of String, System.Enum)
            Dim members As Dictionary(Of String, System.Reflection.FieldInfo) =
               (From fi As System.Reflection.FieldInfo In enumType.GetFields(Reflection.BindingFlags.Public Or Reflection.BindingFlags.Static)
                Select fi).ToDictionary(Of String, System.Reflection.FieldInfo)(Function(current) current.Name, Function(current) current)

            For Each de As KeyValuePair(Of String, System.Reflection.FieldInfo) In members
               Dim value As System.Enum = CType(de.Value.GetValue(Nothing), System.Enum)
               Dim attrs As Object() = de.Value.GetCustomAttributes(GetType(Runtime.Serialization.EnumMemberAttribute), False)

               If attrs IsNot Nothing AndAlso attrs.Length > 0 Then
                  'alias via EnumMemberAttribute defined
                  Dim aliasName As String = CType(attrs(0), srs.EnumMemberAttribute).Value

                  'alias has priority
                  AppendName(_names, value, aliasName)
                  AppendValue(_values, aliasName, value)
               Else
                  AppendName(names, value, de.Key)
               End If
               AppendValue(values, de.Key, value)
            Next
            'append original entries if possible
            For Each de As KeyValuePair(Of System.Enum, String) In names
               If Not _names.ContainsKey(de.Key) Then _names.Add(de.Key, de.Value)
            Next
            For Each de As KeyValuePair(Of String, System.Enum) In values
               If Not _values.ContainsKey(de.Key) Then _values.Add(de.Key, de.Value)
            Next
         End Sub

         Private Sub AppendName(names As Dictionary(Of System.Enum, String), key As System.Enum, name As String)
            If Not names.ContainsKey(key) AndAlso name <> "" Then names.Add(key, name)
         End Sub
         Private Sub AppendValue(values As Dictionary(Of String, System.Enum), key As String, value As System.Enum)
            If key <> "" Then
               If Not values.ContainsKey(key) Then values.Add(key, value)
               key = key.ToLowerInvariant()
               If Not values.ContainsKey(key) Then values.Add(key, value)
               'enumeration value
               Dim arr As Array = Array.CreateInstance(System.Enum.GetUnderlyingType(value.GetType), 1)
               arr.SetValue(value, 0)
               key = arr.GetValue(0).ToString()
               If Not values.ContainsKey(key) Then values.Add(key, value)
            End If
         End Sub

         ''' <summary>
         ''' Gets singleton for specified enumType
         ''' </summary>
         ''' <param name="enumType"></param>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Private Shared Function GetInstance(enumType As Type) As EnumInspector
            SyncLock _instances
               If _instances.ContainsKey(enumType) Then
                  Return _instances(enumType)
               Else
                  Dim retVal As New EnumInspector(enumType)
                  _instances.Add(enumType, retVal)
                  Return retVal
               End If
            End SyncLock
         End Function

         ''' <summary>
         ''' Returns the name of value regarding EnumMemberAttribute
         ''' </summary>
         ''' <param name="value"></param>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Shared Function GetName(value As System.Enum) As String
            Return GetInstance(value.GetType).GetNameCore(value)
         End Function
         Private Function GetNameCore(value As System.Enum) As String
            Return If(_names.ContainsKey(value), _names(value), value.ToString)
         End Function

         ''' <summary>
         ''' Returns True if name is a valid expression
         ''' </summary>
         ''' <typeparam name="TEnum"></typeparam>
         ''' <param name="name"></param>
         ''' <param name="value"></param>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Shared Function TryParse(Of TEnum As Structure)(name As String, ByRef value As TEnum, ignoreCase As Boolean) As Boolean
            Dim enumType As Type = GetType(TEnum)
            Return If(enumType.IsEnum AndAlso name <> "", GetInstance(GetType(TEnum)).TryParseCore(name, value, ignoreCase), False)
         End Function
         Private Function TryParseCore(Of TEnum As Structure)(name As String, ByRef value As TEnum, ignoreCase As Boolean) As Boolean
            If ignoreCase Then name = name.ToLowerInvariant()
            If _values.ContainsKey(name) Then
               value = CType(CObj(_values(name)), TEnum)
               Return True
            Else
               Return System.Enum.TryParse(name, ignoreCase, value)
            End If
         End Function

         Private Shared _instances As New Dictionary(Of Type, EnumInspector)
         Private _names As New Dictionary(Of System.Enum, String)
         Private _values As New Dictionary(Of String, System.Enum)

      End Class
#End Region

#Region "XmlSerialization"
      ''' <summary>
      ''' Convert and ConvertBack from base types to string and vice versa
      ''' </summary>
      ''' <remarks></remarks> 
      Public DefaultXmlConverter As New Dictionary(Of Type, Object) From {
            {GetType(Boolean), New Tuple(Of Func(Of Boolean, String), Func(Of String, Boolean))(
                  Function(item) sx.XmlConvert.ToString(item),
                  Function(item) sx.XmlConvert.ToBoolean(item))},
            {GetType(Boolean?), New Tuple(Of Func(Of Boolean?, String), Func(Of String, Boolean?))(
                  Function(item) If(item.HasValue, sx.XmlConvert.ToString(item.Value), Nothing),
                  Function(item) If(String.IsNullOrEmpty(item), Nothing, sx.XmlConvert.ToBoolean(item)))},
            {GetType(Byte), New Tuple(Of Func(Of Byte, String), Func(Of String, Byte))(
                  Function(item) sx.XmlConvert.ToString(item),
                  Function(item) sx.XmlConvert.ToByte(item))},
            {GetType(Byte?), New Tuple(Of Func(Of Byte?, String), Func(Of String, Byte?))(
                  Function(item) If(item.HasValue, sx.XmlConvert.ToString(item.Value), Nothing),
                  Function(item) If(String.IsNullOrEmpty(item), Nothing, sx.XmlConvert.ToByte(item)))},
            {GetType(DateTime), New Tuple(Of Func(Of DateTime, String), Func(Of String, DateTime))(
                  Function(item) sx.XmlConvert.ToString(item, Xml.XmlDateTimeSerializationMode.Utc),
                  Function(item) sx.XmlConvert.ToDateTime(item, Xml.XmlDateTimeSerializationMode.Utc))},
            {GetType(DateTime?), New Tuple(Of Func(Of DateTime?, String), Func(Of String, DateTime?))(
                  Function(item) If(item.HasValue, sx.XmlConvert.ToString(item.Value, Xml.XmlDateTimeSerializationMode.Utc), Nothing),
                  Function(item) If(String.IsNullOrEmpty(item), Nothing, sx.XmlConvert.ToDateTime(item, Xml.XmlDateTimeSerializationMode.Utc)))},
            {GetType(DateTimeOffset), New Tuple(Of Func(Of DateTimeOffset, String), Func(Of String, DateTimeOffset))(
                  Function(item) sx.XmlConvert.ToString(item),
                  Function(item) CreateDateTimeOffset(item))},
            {GetType(DateTimeOffset?), New Tuple(Of Func(Of DateTimeOffset?, String), Func(Of String, DateTimeOffset?))(
                  Function(item) If(item.HasValue, sx.XmlConvert.ToString(item.Value), Nothing),
                  Function(item) If(String.IsNullOrEmpty(item), Nothing, CreateDateTimeOffset(item)))},
            {GetType(Decimal), New Tuple(Of Func(Of Decimal, String), Func(Of String, Decimal))(
                  Function(item) sx.XmlConvert.ToString(item),
                  Function(item) sx.XmlConvert.ToDecimal(item))},
            {GetType(Decimal?), New Tuple(Of Func(Of Decimal?, String), Func(Of String, Decimal?))(
                  Function(item) If(item.HasValue, sx.XmlConvert.ToString(item.Value), Nothing),
                  Function(item) If(String.IsNullOrEmpty(item), Nothing, sx.XmlConvert.ToDecimal(item)))},
            {GetType(Double), New Tuple(Of Func(Of Double, String), Func(Of String, Double))(
                  Function(item) sx.XmlConvert.ToString(item),
                  Function(item) sx.XmlConvert.ToDouble(item))},
            {GetType(Double?), New Tuple(Of Func(Of Double?, String), Func(Of String, Double?))(
                  Function(item) If(item.HasValue, sx.XmlConvert.ToString(item.Value), Nothing),
                  Function(item) If(String.IsNullOrEmpty(item), Nothing, sx.XmlConvert.ToDouble(item)))},
            {GetType(Int32), New Tuple(Of Func(Of Integer, String), Func(Of String, Integer))(
                  Function(item) sx.XmlConvert.ToString(item),
                  Function(item) sx.XmlConvert.ToInt32(item))},
            {GetType(Int32?), New Tuple(Of Func(Of Int32?, String), Func(Of String, Int32?))(
                  Function(item) If(item.HasValue, sx.XmlConvert.ToString(item.Value), Nothing),
                  Function(item) If(String.IsNullOrEmpty(item), Nothing, sx.XmlConvert.ToInt32(item)))},
            {GetType(Int64), New Tuple(Of Func(Of Int64, String), Func(Of String, Int64))(
                  Function(item) sx.XmlConvert.ToString(item),
                  Function(item) sx.XmlConvert.ToInt64(item))},
            {GetType(Int64?), New Tuple(Of Func(Of Int64?, String), Func(Of String, Int64?))(
                  Function(item) If(item.HasValue, sx.XmlConvert.ToString(item.Value), Nothing),
                  Function(item) If(String.IsNullOrEmpty(item), Nothing, sx.XmlConvert.ToInt64(item)))},
            {GetType(Single), New Tuple(Of Func(Of Single, String), Func(Of String, Single))(
                  Function(item) sx.XmlConvert.ToString(item),
                  Function(item) sx.XmlConvert.ToSingle(item))},
            {GetType(Single?), New Tuple(Of Func(Of Single?, String), Func(Of String, Single?))(
                  Function(item) If(item.HasValue, sx.XmlConvert.ToString(item.Value), Nothing),
                  Function(item) If(String.IsNullOrEmpty(item), Nothing, sx.XmlConvert.ToSingle(item)))},
            {GetType(String), New Tuple(Of Func(Of String, String), Func(Of String, String))(
                  Function(value) value,
                  Function(value) value)}}

      ''' <summary>
      ''' Converts a primitive object into its string-representation
      ''' </summary>
      ''' <typeparam name="T"></typeparam>
      ''' <param name="value"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function Convert(Of T)(value As T) As String
         Return If(DefaultXmlConverter.ContainsKey(GetType(T)),
                   CType(DefaultXmlConverter(GetType(T)), Tuple(Of Func(Of T, String), Func(Of String, T))).Item1(value),
                   If(value Is Nothing, Nothing, value.ToString()))
      End Function

      ''' <summary>
      ''' Converts value into primitive object
      ''' </summary>
      ''' <typeparam name="T"></typeparam>
      ''' <param name="value"></param>
      ''' <param name="defaultValue"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function ConvertBack(Of T)(value As String, defaultValue As T) As T
         Try
            Return If(DefaultXmlConverter.ContainsKey(GetType(T)),
                      CType(DefaultXmlConverter(GetType(T)), Tuple(Of Func(Of T, String), Func(Of String, T))).Item2(value), defaultValue)
         Catch
            Return defaultValue
         End Try
      End Function

      ''' <summary>
      ''' Returns Nothing if the current element is not a startelement, otherwise the name of the element
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetCurrentStartElementLocalName(reader As System.Xml.XmlReader) As String
         reader.MoveToContent()
         Return If(reader.IsStartElement, reader.LocalName, Nothing)
      End Function

      ''' <summary>
      ''' Reads to EndElement of the current node skipping the rest of the childnodes
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <remarks></remarks>
      <System.Runtime.CompilerServices.Extension()>
      Public Sub ReadToEndElement(reader As System.Xml.XmlReader, removeEndElement As Boolean)
         While reader.MoveToContent() <> Xml.XmlNodeType.EndElement
            reader.Skip()
         End While
         If removeEndElement Then reader.ReadEndElement()
      End Sub
#End Region

#Region "Enum-Functions"
      ''' <summary>
      ''' Returns the name of value regarding EnumMemberAttribute
      ''' </summary>
      ''' <param name="value"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <src.Extension()>
      Public Function GetName(value As System.Enum) As String
         Return EnumInspector.GetName(value)
      End Function

      ''' <summary>
      ''' Returns True if the name expression is valid for TEnum-objects
      ''' </summary>
      ''' <typeparam name="TEnum"></typeparam>
      ''' <param name="name"></param>
      ''' <param name="value"></param>
      ''' <param name="ignoreCase"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <src.Extension()>
      Public Function TryParse(Of TEnum As Structure)(name As String, ByRef value As TEnum, Optional ignoreCase As Boolean = False) As Boolean
         Return EnumInspector.TryParse(name, value, ignoreCase)
      End Function

      ''' <summary>
      ''' Returns True if the name expression is valid for enum-type specified by value.GetType
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <src.Extension()>
      Public Function TryParseGeneric(name As String, ByRef value As System.Enum, Optional ignoreCase As Boolean = False) As Boolean
         With GenericRuntimeHelper.GetInstance(value.GetType())
            Return .TryParseGeneric(name, value, ignoreCase)
         End With
      End Function

      ''' <summary>
      ''' Converts enumServiceException to HttpStatusCode
      ''' </summary>
      ''' <param name="value"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <src.Extension()>
      Public Function ToHttpStatusCode(value As Messaging.enumServiceException) As System.Net.HttpStatusCode
         Select Case value
            Case Messaging.enumServiceException.constraint
               Return System.Net.HttpStatusCode.Conflict
            Case Messaging.enumServiceException.contentAlreadyExists
               Return System.Net.HttpStatusCode.Conflict
            Case Messaging.enumServiceException.nameConstraintViolation
               Return System.Net.HttpStatusCode.Conflict
            Case Messaging.enumServiceException.filterNotValid
               Return System.Net.HttpStatusCode.BadRequest
            Case Messaging.enumServiceException.invalidArgument
               Return System.Net.HttpStatusCode.BadRequest
            Case Messaging.enumServiceException.notSupported
               Return System.Net.HttpStatusCode.MethodNotAllowed
            Case Messaging.enumServiceException.objectNotFound
               Return System.Net.HttpStatusCode.NotFound
            Case Messaging.enumServiceException.permissionDenied
               Return System.Net.HttpStatusCode.Forbidden
            Case Messaging.enumServiceException.runtime
               Return System.Net.HttpStatusCode.InternalServerError
            Case Messaging.enumServiceException.storage
               Return System.Net.HttpStatusCode.InternalServerError
            Case Messaging.enumServiceException.streamNotSupported
               Return System.Net.HttpStatusCode.Forbidden
            Case Messaging.enumServiceException.updateConflict
               Return System.Net.HttpStatusCode.Conflict
            Case Messaging.enumServiceException.versioning
               Return System.Net.HttpStatusCode.Conflict
            Case Else
               Return System.Net.HttpStatusCode.InternalServerError
         End Select
      End Function

      ''' <summary>
      ''' Conversion between enumRelationshipDirection and enumIncludeRelationships
      ''' </summary>
      ''' <param name="value"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <src.Extension()>
      Public Function ToIncludeRelationships(value As Core.enumRelationshipDirection) As Core.enumIncludeRelationships
         Select Case value
            Case Core.enumRelationshipDirection.either
               Return Core.enumIncludeRelationships.both
            Case Core.enumRelationshipDirection.source
               Return Core.enumIncludeRelationships.source
            Case Core.enumRelationshipDirection.target
               Return Core.enumIncludeRelationships.target
            Case Else
               Return Core.enumIncludeRelationships.none
         End Select
      End Function

      ''' <summary>
      ''' Converts HttpStatusCode to enumServiceException (lossy)
      ''' </summary>
      <src.Extension()>
      Public Function ToServiceException(value As System.Net.HttpStatusCode) As Messaging.enumServiceException
         Select Case value
            Case Net.HttpStatusCode.BadRequest
               Return Messaging.enumServiceException.invalidArgument
            Case Net.HttpStatusCode.Conflict
               Return Messaging.enumServiceException.constraint
            Case Net.HttpStatusCode.Forbidden
               Return Messaging.enumServiceException.permissionDenied
            Case Net.HttpStatusCode.InternalServerError
               Return Messaging.enumServiceException.runtime
            Case Net.HttpStatusCode.MethodNotAllowed
               Return Messaging.enumServiceException.notSupported
            Case Net.HttpStatusCode.NotFound
               Return Messaging.enumServiceException.objectNotFound
            Case Else
               Return Messaging.enumServiceException.runtime
         End Select
      End Function
#End Region

#Region "ServiceUri-Support"
      ''' <summary>
      ''' Creates a new Uri
      ''' </summary>
      <System.Runtime.CompilerServices.Extension()>
      Public Function Combine(baseUri As Uri, relativeUri As String) As Uri
         If String.IsNullOrEmpty(relativeUri) Then
            Return baseUri
         ElseIf relativeUri = "/" Then
            If baseUri.OriginalString.EndsWith("/"c) Then
               Return baseUri
            Else
               Return New Uri(baseUri.OriginalString & "/")
            End If
         ElseIf relativeUri.StartsWith("?"c) Then
            Return New Uri(baseUri, relativeUri)
         ElseIf relativeUri.StartsWith("/"c) Then
            relativeUri = relativeUri.Substring(1)
         End If
         If baseUri.OriginalString.EndsWith("/"c) Then
            Return New Uri(baseUri, relativeUri)
         Else
            Return New Uri(New Uri(baseUri.OriginalString & "/"), relativeUri)
         End If
      End Function

      ''' <summary>
      ''' Searches the serviceUri for placeHolders and replaces them with given replacements.
      ''' </summary>
      <System.Runtime.CompilerServices.Extension()>
      Public Function ReplaceUri(serviceUri As String, replacements As Dictionary(Of String, String)) As String
         Dim stringQueryParameters As Boolean = False

         If serviceUri = "" OrElse replacements.Count = 0 Then
            Return serviceUri
         Else
            Dim regEx As New System.Text.RegularExpressions.Regex("(\A\/|\?|\{(?<placeHolder>[A-Z][A-Z0-9]*)\})", Text.RegularExpressions.RegexOptions.IgnoreCase)
            Dim evaluator As System.Text.RegularExpressions.MatchEvaluator =
               Function(match)
                  Dim group As System.Text.RegularExpressions.Group = match.Groups("placeHolder")

                  'placeHolder or start of stringqueryparameters found
                  If match.Value = "?" Then
                     stringQueryParameters = True
                     Return "?"
                  ElseIf match.Value = "/" Then
                     'starting slash has to be removed (\A\/)
                     Return ""
                  ElseIf replacements.ContainsKey(group.Value) Then
                     'Return If(stringQueryParameters, System.Web.HttpUtility.UrlEncode(pairs(group.Value)), System.Web.HttpUtility.UrlPathEncode(pairs(group.Value)))
                     Dim replacement As String = replacements(group.Value)
                     Return If(String.IsNullOrEmpty(replacement), replacement, System.Uri.EscapeDataString(replacement))
                  ElseIf replacements.ContainsKey("*") Then
                     'undefined placeHolder with defined generalReplacement
                     'Return If(stringQueryParameters, System.Web.HttpUtility.UrlEncode(pairs("*")), System.Web.HttpUtility.UrlPathEncode(pairs("*")))
                     Dim replacement As String = replacements("*")
                     Return If(String.IsNullOrEmpty(replacement), replacement, System.Uri.EscapeDataString(replacement))
                  Else
                     Return match.Value
                  End If
               End Function

            Return regEx.Replace(serviceUri, evaluator)
         End If
      End Function
      ''' <summary>
      ''' Searches the serviceUri for placeHolders and replaces them with given replacements.
      ''' </summary>
      ''' <param name="serviceUri"></param>
      ''' <param name="searchAndReplace">pairs of replacements: the first value defines the name of
      ''' the placeholder, the second value defines the replacement. If the length of this
      ''' parameter is odd, the last value defines the replacement for all placeHolders not
      ''' found in the search and replace pairs before</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <System.Runtime.CompilerServices.Extension()>
      Public Function ReplaceUri(serviceUri As String, ParamArray searchAndReplace As String()) As String
         Return ReplaceUri(serviceUri, ConvertSearchAndReplace(searchAndReplace))
      End Function

      ''' <summary>
      ''' Searches the serviceUri for placeHolders and replaces them with given replacements.
      ''' Unused querystring parameters will be removed
      ''' </summary>
      ''' <param name="serviceUri"></param>
      ''' <param name="searchAndReplace"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <System.Runtime.CompilerServices.Extension()>
      Public Function ReplaceUriTemplate(serviceUri As String, ParamArray searchAndReplace As String()) As String
         'expression detects empty querystring-parameters
         Dim regEx As New System.Text.RegularExpressions.Regex("((?<Query>\?)[^\=\&]+\=(\&[^\=\&]+\=(?=($|\&)))*(?=($|\&))(?<Eos>$)?|\&[^\=\&]+=(?=($|\&)))",
                                                               Text.RegularExpressions.RegexOptions.Singleline)
         Dim evaluator As System.Text.RegularExpressions.MatchEvaluator =
            Function(match)
               Dim group As System.Text.RegularExpressions.Group = match.Groups("Eos")
               Return If(match.Value.StartsWith("?"c) AndAlso (group Is Nothing OrElse Not group.Success), "?", Nothing)
            End Function
         With New List(Of String)
            If searchAndReplace IsNot Nothing Then .AddRange(searchAndReplace)
            'add null-replacement for undefined parameters
            If ((.Count And 1) = 0) Then .Add(Nothing)
            Return regEx.Replace(ReplaceUri(serviceUri, .ToArray()), evaluator)
         End With
      End Function

      ''' <summary>
      ''' Returns the searchAndReplace definitions in the KeyValuePair-form
      ''' </summary>
      ''' <param name="searchAndReplace">If the length of searchAndReplace is odd, the last
      ''' element is interpreted as generalReplacement for all placeHolders not defined before</param>
      ''' <returns></returns>
      ''' <remarks>The generalReplacement is stored with the key '*'</remarks>
      Public Function ConvertSearchAndReplace(searchAndReplace As String()) As Dictionary(Of String, String)
         Dim length As Integer = If(searchAndReplace Is Nothing, 0, searchAndReplace.Length)
         Dim retVal As New Dictionary(Of String, String)(Math.Max(1, (length + 1) >> 1))

         If length > 0 Then
            Dim pairs As New Queue(Of String)(searchAndReplace)

            While pairs.Count > 1
               Dim placeHolderName As String = pairs.Dequeue
               Dim replacement As String = pairs.Dequeue
               If placeHolderName <> "" AndAlso Not retVal.ContainsKey(placeHolderName) Then retVal.Add(placeHolderName, replacement)
            End While
            If pairs.Count > 0 AndAlso Not retVal.ContainsKey("*") Then retVal.Add("*", pairs.Dequeue)
         End If

         Return retVal
      End Function
#End Region

#Region "ElementName-Support"
      ''' <summary>
      ''' Returns the XmlRootAttribute for class T
      ''' </summary>
      ''' <typeparam name="T"></typeparam>
      ''' <param name="inherit"></param>
      ''' <param name="exactNonNullResult">If True: if there is no XmlRootAttribute defined for class T,
      ''' an empty XmlRootAttribute is returned</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetXmlRootAttribute(Of T)(Optional inherit As Boolean = False,
                                                Optional exactNonNullResult As Boolean = False) As System.Xml.Serialization.XmlRootAttribute
         Return GetXmlRootAttribute(GetType(T), inherit, exactNonNullResult)
      End Function
      ''' <summary>
      ''' Returns the XmlRootAttribute for instanceOrType
      ''' </summary>
      ''' <param name="instanceOrType"></param>
      ''' <param name="inherit"></param>
      ''' <param name="exactNonNullResult">If True: if there is no XmlRootAttribute defined for instanceOrType,
      ''' an empty XmlRootAttribute is returned</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <System.Runtime.CompilerServices.Extension()>
      Public Function GetXmlRootAttribute(instanceOrType As Object, Optional inherit As Boolean = False,
                                          Optional exactNonNullResult As Boolean = False) As System.Xml.Serialization.XmlRootAttribute
         If instanceOrType IsNot Nothing Then
            Dim type As Type = If(TypeOf instanceOrType Is Type, CType(instanceOrType, Type), instanceOrType.GetType())
            Dim attrs As Object() = type.GetCustomAttributes(GetType(System.Xml.Serialization.XmlRootAttribute), inherit)
            If attrs IsNot Nothing AndAlso attrs.Length > 0 Then
               Return CType(attrs(0), System.Xml.Serialization.XmlRootAttribute)
            ElseIf exactNonNullResult Then
               Return New System.Xml.Serialization.XmlRootAttribute()
            Else
               Return Nothing
            End If
         ElseIf exactNonNullResult Then
            Return New System.Xml.Serialization.XmlRootAttribute()
         Else
            Return Nothing
         End If
      End Function

      ''' <summary>
      ''' Returns the root elementname defined by System.Xml.Serialization.XmlRootAttribute
      ''' </summary>
      ''' <typeparam name="T"></typeparam>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetXmlRootElementName(Of T)(Optional inherit As Boolean = False) As String
         Return GetXmlRootAttribute(Of T)(inherit, True).ElementName
      End Function
      ''' <summary>
      ''' Returns the root elementname defined by System.Xml.Serialization.XmlRootAttribute
      ''' </summary>
      ''' <param name="instanceOrType"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <System.Runtime.CompilerServices.Extension()>
      Public Function GetXmlRootElementName(instanceOrType As Object, Optional inherit As Boolean = False) As String
         Return GetXmlRootAttribute(instanceOrType, inherit, True).ElementName
      End Function

      ''' <summary>
      ''' Returns the root namespace defined by System.Xml.Serialization.XmlRootAttribute
      ''' </summary>
      ''' <typeparam name="T"></typeparam>
      ''' <param name="inherit"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetXmlRootNamespace(Of T)(Optional inherit As Boolean = False) As String
         Return GetXmlRootAttribute(Of T)(inherit, True).Namespace
      End Function
      ''' <summary>
      ''' Returns the root namespace defined by System.Xml.Serialization.XmlRootAttribute
      ''' </summary>
      ''' <param name="instanceOrType"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <System.Runtime.CompilerServices.Extension()>
      Public Function GetXmlRootNamespace(instanceOrType As Type, Optional inherit As Boolean = False) As String
         Return GetXmlRootAttribute(instanceOrType, inherit, True).Namespace
      End Function

      ''' <summary>
      ''' Returns the nodename without namespace information
      ''' </summary>
      ''' <param name="node"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <System.Runtime.CompilerServices.Extension()>
      Public Function GetNodeNameWithoutNamespace(node As Xml.XmlNode) As String
         Dim name As String = node.Name
         Dim indexOf As Integer = name.IndexOf(":"c)
         Return If(indexOf < 0, name, name.Substring(indexOf + 1))
      End Function

      ''' <summary>
      ''' Returns the namespace of node
      ''' </summary>
      ''' <param name="node"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <System.Runtime.CompilerServices.Extension()>
      Public Function GetNodeNamespace(node As Xml.XmlNode) As String
         Dim name As String = node.Name
         Dim indexOf As Integer = name.IndexOf(":"c)
         Return If(indexOf <= 0, Nothing, name.Substring(0, indexOf))
      End Function
#End Region

#Region "Parse Nullables"
      ''' <summary>
      ''' Converts a string to a nullable integer.
      ''' </summary>
      ''' <param name="value"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function ParseInteger(value As String) As xs_Integer?
         Dim intValue As xs_Integer

         If String.IsNullOrEmpty(value) OrElse Not xs_Integer.TryParse(value, intValue) Then
            Return Nothing
         Else
            Return intValue
         End If
      End Function
      ''' <summary>
      '''  Converts a string to a nullable boolean.
      ''' </summary>
      ''' <param name="value"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function ParseBoolean(value As String) As Boolean?
         Dim boolValue As Boolean

         If String.IsNullOrEmpty(value) OrElse Not Boolean.TryParse(value, boolValue) Then
            Return Nothing
         Else
            Return boolValue
         End If
      End Function
      ''' <summary>
      '''  Converts a string to a nullable enum.
      ''' </summary>
      ''' <typeparam name="T"></typeparam>
      ''' <param name="value"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function ParseEnum(Of T As {Structure})(value As String) As T?
         Dim enumValue As T = Nothing

         If String.IsNullOrEmpty(value) OrElse Not System.Enum.TryParse(value, enumValue) Then
            Return Nothing
         Else
            Return enumValue
         End If
      End Function
#End Region

#Region "DateTime"
      Private _toDateTimeRegEx As New System.Text.RegularExpressions.Regex("-?\d{4,}(-\d{2}){2}T\d{2}(\:\d{2}){2}(\.\d+)?([\+\-]\d{2}\:\d{2}|Z)?",
                                                                           Text.RegularExpressions.RegexOptions.Singleline Or
                                                                           Text.RegularExpressions.RegexOptions.IgnoreCase)
      ''' <summary>
      ''' Evaluates the current node as a DateTimeOffset-value
      ''' </summary>
      ''' <param name="dateTimeOffset"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function CreateDateTimeOffset(dateTimeOffset As String) As DateTimeOffset
         SyncLock _toDateTimeRegEx
            Try
               Dim isNullOrEmpty As Boolean = String.IsNullOrEmpty(dateTimeOffset)
               Dim match As System.Text.RegularExpressions.Match = If(isNullOrEmpty, Nothing, _toDateTimeRegEx.Match(dateTimeOffset))

               Return If(isNullOrEmpty, Nothing, If(match Is Nothing OrElse Not match.Success,
                                                    New DateTimeOffset(CDate(dateTimeOffset)),
                                                    sx.XmlConvert.ToDateTimeOffset(dateTimeOffset)))
            Catch
            End Try
         End SyncLock
      End Function

      Private _jsonReferenceDate As New DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)

      <System.Runtime.CompilerServices.Extension()>
      Public Function FromJSONTime(value As Long) As DateTime
         Return _jsonReferenceDate.Add(TimeSpan.FromMilliseconds(value)).ToLocalTime
      End Function

      <System.Runtime.CompilerServices.Extension()>
      Public Function ToJSONTime(value As DateTime) As Long
         Return CLng(value.ToUniversalTime.Subtract(_jsonReferenceDate).TotalMilliseconds)
      End Function
#End Region

#Region "Change-Token"
      ''' <summary>
      ''' ChangeToken-Generator
      ''' </summary>
      ''' <remarks></remarks>
      Public MustInherit Class ChangeToken
         Private Sub New()
         End Sub
         Public MustInherit Class Oracle
            Private Sub New()
            End Sub
            ''' <summary>
            ''' Expression for current UTC-time
            ''' </summary>
            ''' <remarks></remarks>
            Public Const NextChangeTokenExpression As String = "To_Char(sys_extract_utc(systimestamp),'YYYYMMDDHH24MISSFF')"
         End Class
         Public Shared ReadOnly Property NextChangeToken As String
            Get
               Return Format(Date.UtcNow, "yyyyMMddHHmmssffffff000")
            End Get
         End Property
      End Class
#End Region

#Region "Support cmisPropertyDecimal"
      Private _allowedDecimalRepresentations As New HashSet(Of enumDecimalRepresentation) From {
         enumDecimalRepresentation.decimal, enumDecimalRepresentation.double}
      Private _decimalRepresentation As enumDecimalRepresentation = enumDecimalRepresentation.decimal
      Public Property DecimalRepresentation As enumDecimalRepresentation
         Get
            Return _decimalRepresentation
         End Get
         Set(value As enumDecimalRepresentation)
            If value <> _decimalRepresentation AndAlso _allowedDecimalRepresentations.Contains(value) Then
               _decimalRepresentation = value
               RaiseEvent DecimalRepresentationChanged(value)
            End If
         End Set
      End Property

      Public Event DecimalRepresentationChanged(newValue As enumDecimalRepresentation)
#End Region

#Region "GenericType-Support"
      ''' <summary>
      ''' Returns true if genericTypeDefinition is the direct or indirect generic type definition of type
      ''' </summary>
      ''' <param name="type"></param>
      ''' <param name="genericTypeDefinition"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <src.Extension()>
      Public Function BasedOnGenericTypeDefinition(type As Type, genericTypeDefinition As Type) As Boolean
         Dim nearestGenericTypeDefinition = FindGenericTypeDefinition(type)

         If nearestGenericTypeDefinition Is genericTypeDefinition Then
            Return True
         ElseIf nearestGenericTypeDefinition Is Nothing Then
            Return False
         Else
            Return BasedOnGenericTypeDefinition(nearestGenericTypeDefinition.BaseType, genericTypeDefinition)
         End If
      End Function

      ''' <summary>
      ''' Returns the generic type definition which type is based on, or nothing, if no generic type definition could be found
      ''' </summary>
      ''' <param name="type"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <src.Extension()>
      Public Function FindGenericTypeDefinition(type As Type) As Type
         If type Is Nothing OrElse type Is GetType(Object) Then
            Return Nothing
         ElseIf type.IsGenericType Then
            Return type.GetGenericTypeDefinition()
         Else
            Return FindGenericTypeDefinition(type.BaseType)
         End If
      End Function

      ''' <summary>
      ''' Converts dictionary to IDictionary(Of TKey, TValue)
      ''' </summary>
      ''' <remarks>dictionary must be a IDictionary(Of) instance and the keys must be of type TKey or derived from TKey
      ''' and the values must be of type TValue or derived from TValue</remarks>
      Public Function GeneralizeDictionary(Of TKey, TValue)(dictionary As Object) As IDictionary(Of TKey, TValue)
         'suppose keys are of type string (if not the genericRuntimeHelper will select the correct keyType)
         Return GenericRuntimeHelper.GetInstance(GetType(String)).ConvertDictionary(Of TKey, TValue)(dictionary)
      End Function

      ''' <summary>
      ''' Returns True if dictionary could be converted to IDictionary(Of TKey, TValue)
      ''' </summary>
      ''' <param name="dictionary">If successful the value is changed to IDictionary(Of TKey, TValue) type</param>
      Public Function TryConvertDictionary(Of TKey, TValue)(ByRef dictionary As Object) As Boolean
         'suppose keys are of type string (if not the genericRuntimeHelper will select the correct keyType)
         Return GenericRuntimeHelper.GetInstance(GetType(String)).TryConvertDictionary(Of TKey, TValue)(dictionary)
      End Function
#End Region

#Region "Request-Support"
      ''' <summary>
      ''' Returns the queryStringParameter that corresponds with the name of enumValue
      ''' </summary>
      ''' <typeparam name="TEnum"></typeparam>
      ''' <param name="enumValue"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetRequestParameter(Of TEnum As Structure)(enumValue As TEnum) As String
         Dim enumType As Type = GetType(TEnum)

         If enumType.IsEnum Then
            Return GetRequestParameter(ServiceURIs.GetValues(enumType).Item(CInt(CObj(enumValue))).Item1)
         Else
            Return Nothing
         End If
      End Function
      ''' <summary>
      ''' Returns the queryStringParameter that corresponds with parameterName
      ''' </summary>
      ''' <param name="parameterName"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetRequestParameter(parameterName As String) As String
         Dim requestParams As System.Collections.Specialized.NameValueCollection = If(ssw.WebOperationContext.Current Is Nothing, Nothing,
                                                                                      ssw.WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters)
         If requestParams IsNot Nothing Then
            Dim retVal As String = requestParams(parameterName)
            Return If(String.IsNullOrEmpty(retVal), Nothing, retVal)
         Else
            Return Nothing
         End If
      End Function

      ''' <summary>
      ''' Returns the query-parameters of the incoming request
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetRequestParameters() As System.Collections.Generic.Dictionary(Of String, String)
         Dim retVal As New System.Collections.Generic.Dictionary(Of String, String)

         If ssw.WebOperationContext.Current IsNot Nothing Then
            Dim queryParameters As System.Collections.Specialized.NameValueCollection = ssw.WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters
            If queryParameters IsNot Nothing AndAlso queryParameters.Count > 0 Then
               For index As Integer = 0 To queryParameters.Count - 1
                  Dim key As String = If(queryParameters.GetKey(index), "")
                  If Not retVal.ContainsKey(key) Then retVal.Add(key, queryParameters.Get(index))
               Next
            End If
         End If

         Return retVal
      End Function
#End Region

#Region "PWCRemovedHandler-Support"
      ''' <summary>
      ''' Creates WeakListeners to detect the end of a checkedOut-state.
      ''' </summary>
      <src.Extension()>
      Public Sub AddPWCRemovedListeners(handler As EventBus.WeakListenerCallback, ByRef listeners As EventBus.WeakListener(),
                                        absoluteUri As String, repositoryId As String, pwcId As String)
         Dim eventNames As String() = New String() {EventBus.EndCancelCheckout, EventBus.EndCheckIn, EventBus.EndDeleteObject}
         Dim length As Integer = eventNames.Length

         SyncLock handler
            RemovePWCRemovedListeners(handler, listeners)
            listeners = DirectCast(System.Array.CreateInstance(GetType(EventBus.WeakListener), length), EventBus.WeakListener())
            For index As Integer = 0 To length - 1
               listeners(index) = EventBus.WeakListener.CreateInstance(handler, absoluteUri, repositoryId, eventNames(index), pwcId)
            Next
         End SyncLock
      End Sub

      ''' <summary>
      ''' Releases listeners
      ''' </summary>
      <src.Extension()>
      Public Sub RemovePWCRemovedListeners(handler As EventBus.WeakListenerCallback, ByRef listeners As EventBus.WeakListener())
         SyncLock handler
            If listeners IsNot Nothing Then
               For index As Integer = 0 To listeners.Length - 1
                  Dim listener As EventBus.WeakListener = listeners(index)
                  If listener IsNot Nothing Then listener.RemoveListener()
               Next
               listeners = Nothing
            End If
         End SyncLock
      End Sub
#End Region

      ''' <summary>
      ''' Copies given array starting with the element on position index
      ''' </summary>
      ''' <typeparam name="TItem"></typeparam>
      ''' <param name="array"></param>
      ''' <param name="index"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <src.Extension()>
      Public Function Copy(Of TItem)(array As TItem(), Optional index As Integer = 0) As TItem()
         Dim length As Integer = Math.Max(0, If(array Is Nothing, 0, array.Length - Math.Max(0, index)))
         Dim retVal As TItem() = CType(System.Array.CreateInstance(GetType(TItem), length), TItem())

         If length > 0 Then array.CopyTo(retVal, index)
         Return retVal
      End Function

      ''' <summary>
      ''' Creates valid regular expression pattern for text
      ''' </summary>
      ''' <param name="text"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function CreateRegExPattern(ByVal text As String) As String
         Dim regEx As New System.Text.RegularExpressions.Regex("((?<CrLf>(\r\n|\r|\n))|(?<Blank>\s)|\\|\/|\.|\*|\+|\-|\(|\)|\[|\]|\<|\>|\||\?|\$|\{|\}|\^)", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
         Static matchEvaluator As New System.Text.RegularExpressions.MatchEvaluator(AddressOf CreateRegExPatternReplace)

         Return regEx.Replace(text, matchEvaluator)
      End Function
      Private Function CreateRegExPatternReplace(ByVal match As System.Text.RegularExpressions.Match) As String
         Dim grBlank As System.Text.RegularExpressions.Group = match.Groups("Blank")
         Dim grCrLf As System.Text.RegularExpressions.Group = match.Groups("CrLf")
         If grBlank IsNot Nothing AndAlso grBlank.Success Then
            Return "\s"
         ElseIf grCrLf IsNot Nothing AndAlso grCrLf.Success Then
            Select Case grCrLf.Value
               Case vbLf
                  Return "\n"
               Case vbCr
                  Return "\r"
               Case Else
                  Return "(\r\n|\r|\n)"
            End Select
         Else
            Return "\" & match.Value
         End If
      End Function

      ''' <summary>
      ''' Creates a typesafe array of type
      ''' </summary>
      <src.Extension()>
      Public Function CreateValuesArray(type As Type, ParamArray values As Object()) As Array
         Dim length As Integer = If(values Is Nothing, 0, values.Length)
         Dim retVal As Array = Array.CreateInstance(type, length)

         If length > 0 Then
            For index As Integer = 0 To length - 1
               retVal.SetValue(TryCastDynamic(values(index), type, Nothing), index)
            Next
         End If

         Return retVal
      End Function

      Private _defaultLogFile As String

      ''' <summary>
      ''' Mapping between the used namespaces in the cmis-environment and their prefixes in
      ''' xml - documents.
      ''' </summary>
      ''' <remarks></remarks>
      Private ReadOnly _defaultNamespacePrefixes As Dictionary(Of String, String)
      Public Function GetDefaultPrefix(namespaceUri As String, ByRef unknownUriIndex As Integer) As String
         If String.IsNullOrEmpty(namespaceUri) Then
            Return Nothing
         Else
            Dim key As String = namespaceUri.ToLowerInvariant()

            If _defaultNamespacePrefixes.ContainsKey(key) Then
               Return _defaultNamespacePrefixes(key)
            Else
               GetDefaultPrefix = "ns" & unknownUriIndex
               unknownUriIndex += 1
            End If
         End If
      End Function

      ''' <summary>
      ''' If the directory doesn't exist, the method tries to create it and
      ''' returns True, if the directoryPath points to an existing directory.
      ''' </summary>
      ''' <param name="directoryPath"></param>
      ''' <remarks></remarks>
      Public Function EnsureDirectory(directoryPath As String) As Boolean
         If String.IsNullOrEmpty(directoryPath) OrElse IO.Directory.Exists(directoryPath) Then
            'if directoryPath = "" then the current directory is meant
            Return True
         ElseIf EnsureDirectory(IO.Path.GetDirectoryName(directoryPath)) Then
            'parent directory exists
            Try
               Return IO.Directory.CreateDirectory(directoryPath) IsNot Nothing
            Catch ex As Exception
               Return False
            End Try
         Else
            Return False
         End If
      End Function

      Private _jsonTypes As New Dictionary(Of Type, Type) From {
         {GetType(DateTime), GetType(Long)}, {GetType(DateTimeOffset), GetType(Long)},
         {GetType(DateTime?), GetType(Long?)}, {GetType(DateTimeOffset?), GetType(Long?)},
         {GetType(DateTime()), GetType(Long())}, {GetType(DateTimeOffset()), GetType(Long())}}
      ''' <summary>
      ''' Returns the System.Type which is used to represent type in JavaScript-serialization
      ''' </summary>
      ''' <param name="type"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetJSONType(type As Type) As Type
         SyncLock _jsonTypes
            Return If(_jsonTypes.ContainsKey(type), _jsonTypes(type), type)
         End SyncLock
      End Function

      ''' <summary>
      ''' Used namespaces in the cmis-environment
      ''' </summary>
      ''' <remarks></remarks>
      Private _namespaces As New Dictionary(Of sx.XmlQualifiedName, String) From {
         {New sx.XmlQualifiedName("cmis", Constants.Namespaces.xmlns), Constants.Namespaces.cmis},
         {New sx.XmlQualifiedName("cmism", Constants.Namespaces.xmlns), Constants.Namespaces.cmism},
         {New sx.XmlQualifiedName("atom", Constants.Namespaces.xmlns), Constants.Namespaces.atom},
         {New sx.XmlQualifiedName("app", Constants.Namespaces.xmlns), Constants.Namespaces.app},
         {New sx.XmlQualifiedName("cmisra", Constants.Namespaces.xmlns), Constants.Namespaces.cmisra},
         {New sx.XmlQualifiedName("xsi", Constants.Namespaces.xmlns), Constants.Namespaces.w3instance},
         {New sx.XmlQualifiedName("cmisl", Constants.Namespaces.xmlns), Constants.Namespaces.cmisl},
         {New sx.XmlQualifiedName("cmisw", Constants.Namespaces.xmlns), Constants.Namespaces.cmisw},
         {New sx.XmlQualifiedName("com", Constants.Namespaces.xmlns), Constants.Namespaces.com},
         {New sx.XmlQualifiedName("alf", Constants.Namespaces.xmlns), Constants.Namespaces.alf},
         {New sx.XmlQualifiedName("browser", Constants.Namespaces.xmlns), Constants.Namespaces.browser}}
      Public ReadOnly Property CmisNamespaces As Dictionary(Of sx.XmlQualifiedName, String)
         Get
            Return CmisNamespaces("cmis", "cmism", "atom", "app", "cmisra", "xsi")
         End Get
      End Property
      Public ReadOnly Property CmisNamespaces(ParamArray namespaces As String()) As Dictionary(Of sx.XmlQualifiedName, String)
         Get
            If namespaces Is Nothing OrElse namespaces.Length = 0 Then
               Return _namespaces
            Else
               Dim verify As New HashSet(Of String)(namespaces)
               Return (From de As KeyValuePair(Of sx.XmlQualifiedName, String) In _namespaces
                       Where verify.Contains(de.Key.Name)
                       Select de).ToDictionary(Of sx.XmlQualifiedName, String)(Function(item) item.Key, Function(item) item.Value)
            End If
         End Get
      End Property

      ''' <summary>
      ''' Returns True if type is nullable
      ''' </summary>
      ''' <param name="type"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <System.Runtime.CompilerServices.Extension()>
      Public Function IsNullableType(type As Type) As Boolean
         Return type.IsValueType AndAlso type.IsGenericType AndAlso type.GetGenericTypeDefinition Is GetType(Boolean?).GetGenericTypeDefinition()
      End Function

      ''' <summary>
      ''' Logs a message into the logfile configured in AppSettings("LogFile")
      ''' </summary>
      ''' <param name="message"></param>
      ''' <remarks></remarks>
      Public Sub LogMessage(message As String)
         If Not String.IsNullOrEmpty(_defaultLogFile) AndAlso Not String.IsNullOrEmpty(message) Then
            Try
               Dim prefix As String = Now.ToString & " "
               Dim indent As New String(" "c, prefix.Length)

               IO.File.AppendAllText(_defaultLogFile, prefix & message.Replace(vbNewLine, indent & vbNewLine) & vbNewLine)
            Catch
            End Try
         End If
      End Sub

      ''' <summary>
      ''' Logs an error into the logfile configured in AppSettings("LogFile")
      ''' </summary>
      ''' <param name="ex"></param>
      ''' <remarks></remarks>
      Public Sub LogError(ex As Exception)
         Dim indent As String = ""
         Dim sb As New System.Text.StringBuilder

         While ex IsNot Nothing
            sb.AppendLine(indent & ex.Message.Replace(vbNewLine, vbNewLine & indent))
            sb.AppendLine(indent & "StackTrace:")
            indent &= "  "
            sb.AppendLine(indent & ex.StackTrace.Replace(vbNewLine, vbNewLine & indent))
            ex = ex.InnerException
         End While
         If sb.Length > 0 Then sb.Length -= vbNewLine.Length
         LogMessage(sb.ToString())
      End Sub

      ''' <summary>
      ''' Returns a non nullOrEmpty-String if available
      ''' Preference: arg1, arg2
      ''' </summary>
      ''' <param name="arg1"></param>
      ''' <param name="arg2"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <src.Extension()>
      Public Function NVL(arg1 As String, arg2 As String) As String
         If Not String.IsNullOrEmpty(arg1) Then
            Return arg1
         Else
            Return If(arg2, arg1)
         End If
      End Function
      ''' <summary>
      ''' Returns a non nullOrEmpty-String if available
      ''' Preference: arg1, arg2, alternate
      ''' </summary>
      ''' <param name="arg1"></param>
      ''' <param name="arg2"></param>
      ''' <param name="alternate"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <src.Extension()>
      Public Function NVL(arg1 As String, arg2 As String, alternate As String) As String
         If Not String.IsNullOrEmpty(arg1) Then
            Return arg1
         Else
            Return If(NVL(arg2, alternate), arg1)
         End If
      End Function

      ''' <summary>
      ''' Returns instance
      ''' </summary>
      ''' <typeparam name="T"></typeparam>
      ''' <param name="instance"></param>
      ''' <returns></returns>
      ''' <remarks>Useful in With-End With-expressions</remarks>
      <src.Extension()>
      Public Function Self(Of T)(instance As T) As T
         Return instance
      End Function

      ''' <summary>
      ''' Returns a typesafe PropertyChangedEventArgs-object
      ''' </summary>
      ''' <typeparam name="TProperty"></typeparam>
      ''' <param name="propertyName"></param>
      ''' <param name="newValue"></param>
      ''' <param name="oldValue"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <src.Extension()>
      Public Function ToPropertyChangedEventArgs(Of TProperty)(propertyName As String, newValue As TProperty, oldValue As TProperty) As ComponentModel.PropertyChangedEventArgs
         Return New ComponentModel.Generic.PropertyChangedEventArgs(Of TProperty)(propertyName, newValue, oldValue)
      End Function

      Private _tryCastDynamics As New Dictionary(Of Type, Dictionary(Of Type, Boolean))
      ''' <summary>
      ''' Try to convert value to targetType
      ''' </summary>
      <src.Extension()>
      Public Function TryCastDynamic(value As Object, targetType As Type, Optional defaultValue As Object = Nothing) As Object
         Dim sourceType As Type = If(value Is Nothing, GetType(Void), value.GetType())

         If value IsNot Nothing AndAlso value.GetType().IsArray AndAlso targetType.IsArray Then
            Dim targetElementType As Type = targetType.GetElementType()
            Dim sourceElementType As Type = sourceType.GetElementType()

            If sourceElementType Is targetElementType Then
               'nothing to do
               Return value
            Else
               'create a new array type
               Dim source As Array = CType(value, Array)
               Dim length As Integer = source.Length
               Dim retVal As Array = Array.CreateInstance(targetElementType, length)

               For index As Integer = 0 To length - 1
                  retVal.SetValue(TryCastDynamic(source.GetValue(index), targetElementType), index)
               Next
               Return retVal
            End If
         Else
            'try to convert via CTypeDynamic
            'if it is the first try to convert from sourceType to targetType
            'the framework stores if an exception was thrown when CTypeDynamic
            'was called. In this case this function will not allow further
            'tries in the future.
            Dim tryCastDynamics As Dictionary(Of Type, Boolean)
            Dim canConvert As Boolean

            'minimize lock time for all types
            SyncLock _tryCastDynamics
               If _tryCastDynamics.ContainsKey(sourceType) Then
                  tryCastDynamics = _tryCastDynamics(sourceType)
               Else
                  tryCastDynamics = New Dictionary(Of Type, Boolean)
                  _tryCastDynamics.Add(sourceType, tryCastDynamics)
               End If
            End SyncLock
            'minimize lock time for targetType
            SyncLock tryCastDynamics
               If Not tryCastDynamics.ContainsKey(targetType) Then
                  'optimistic
                  tryCastDynamics.Add(targetType, True)
                  canConvert = True
               Else
                  canConvert = tryCastDynamics(targetType)
               End If
            End SyncLock

            If canConvert Then
               Try
                  Return CTypeDynamic(value, targetType)
               Catch
                  If targetType Is GetType(DateTimeOffset) AndAlso TypeOf value Is DateTime Then
                     Return defaultValue
                  End If
                  'if at least one try fails, there will be
                  'no conversations from sourceType to
                  'targetType in future calls
                  SyncLock tryCastDynamics
                     tryCastDynamics(targetType) = False
                  End SyncLock
               End Try
            End If
         End If

         Return defaultValue
      End Function

      ''' <summary>
      ''' Try to convert value to targetType
      ''' </summary>
      <src.Extension()>
      Public Function TryCastDynamic(Of TResult)(value As Object, Optional defaultValue As TResult = Nothing) As TResult
         Return CType(TryCastDynamic(value, GetType(TResult), defaultValue), TResult)
      End Function

      ''' <summary>
      ''' Unwraps the content of the given xmlDoc instance if the DocumentElement contains a base64-string
      ''' </summary>
      ''' <param name="xmlDoc"></param>
      ''' <remarks></remarks>
      <src.Extension()>
      Public Sub UnWrap(ByVal xmlDoc As Xml.XmlDocument)
         If xmlDoc IsNot Nothing AndAlso xmlDoc.DocumentElement IsNot Nothing AndAlso
            String.Compare(xmlDoc.DocumentElement.Name, "Binary", True) = 0 Then
            Try
               xmlDoc.LoadXml(System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(xmlDoc.DocumentElement.InnerText)))
            Catch
            End Try
         End If
      End Sub

      ''' <summary>
      ''' Query names MUST NOT contain " ", ",", """, "'", "\", ".", "(" or ")". This function eliminates these chars.
      ''' </summary>
      ''' <param name="queryName"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <src.Extension()>
      Public Function ValidateQueryName(queryName As String) As String
         If String.IsNullOrEmpty(queryName) Then
            Return queryName
         Else
            Return (New System.Text.RegularExpressions.Regex("[\s,""'\\\.\(\)]", Text.RegularExpressions.RegexOptions.Singleline)).Replace(queryName, "")
         End If
      End Function

   End Module
End Namespace