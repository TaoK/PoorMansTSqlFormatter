VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "WinMergeScript"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = True
'Poor Man 's T-SQL Formatter - a small free Transact-SQL formatting
'library for .Net 2.0, written in C#.
'Copyright (C) 2011 Tao Klerks
'
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU Affero General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.
'
'This program is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU Affero General Public License for more details.
'
'You should have received a copy of the GNU Affero General Public License
'along with this program.  If not, see <http://www.gnu.org/licenses/>.
'

Option Explicit

Public Property Get PluginEvent() As String
    'Haven't figured out whether there's an advantage to either approach...
    PluginEvent = "BUFFER_PACK_UNPACK"
    'PluginEvent = "FILE_PACK_UNPACK"
End Property

Public Property Get PluginDescription() As String
    PluginDescription = "This plugin reformats T-SQL code according to a defined standard, so that the difference between two files shows the changes in content, not format. PLEASE NOTE: text encoding defaults to UTF-8, and may not display extended characters in legacy encodings such as Windows-1252."
End Property

Public Property Get PluginFileFilters() As String
    PluginFileFilters = "\.sql$"
End Property

Public Property Get PluginIsAutomatic() As Boolean
    PluginIsAutomatic = True
End Property
  
Private Function BytesFromString(ByRef strText As String, ByVal encoding As String, ByVal skipBOM As Boolean) As Byte()
    Dim objStream As ADODB.Stream
    Dim data() As Byte
    
    Set objStream = New ADODB.Stream
    With objStream
        .Charset = encoding
        .Mode = adModeReadWrite
        .Type = adTypeText
        .Open
    
        .WriteText strText
        .Flush
    
        .Position = 0
        .Type = adTypeBinary
        
        If UCase(encoding) = "UTF-8" And skipBOM Then
            .Read 3
        ElseIf (UCase(encoding) = "UNICODE" Or UCase(encoding) = "UTF-16") And skipBOM Then
            .Read 2
        End If
        data = objStream.Read()
    
        .Close
    End With
    
    BytesFromString = data
End Function

Private Function StringFromBytes(ByRef data() As Byte, encoding As String) As String
    Dim objStream As ADODB.Stream
    Dim strTmp As String
    
    Set objStream = New ADODB.Stream
    With objStream
        .Charset = encoding
        .Mode = adModeReadWrite
        .Type = adTypeBinary
        .Open
    
        .Write data
        .Flush
    
        .Position = 0
        .Type = adTypeText
        strTmp = .ReadText
    
        .Close
    End With
    
    StringFromBytes = strTmp
End Function

Private Sub ReadByteArray(ByVal strPath As String, ByRef arrData() As Byte)
    Dim lngFile As Long
    lngFile = FreeFile()
    Open strPath For Binary Access Read As lngFile
    ReDim arrData(1 To LOF(lngFile)) As Byte
    Get lngFile, 0, arrData
    Close lngFile
End Sub

Private Sub WriteByteArray(ByVal strPath As String, ByRef arrData() As Byte)
    Dim lngFile As Long
    lngFile = FreeFile()
    Open strPath For Binary Access Write As lngFile
    Put lngFile, 0, arrData
    Close lngFile
End Sub

Private Function ReformatByteBuffer(ByRef buffer() As Byte)

    Dim originalString As String
    Dim skipOutBOM As Boolean
    Dim bufferStart As Integer
    bufferStart = LBound(buffer)
    If (buffer(bufferStart + 0) = 254 And buffer(bufferStart + 1) = 255) Or (buffer(bufferStart + 0) = 255 And buffer(bufferStart + 1) = 254) Then
        'As long as there is a BOM, ADODB is happy to distinguish LE from BE automatically.
        originalString = StringFromBytes(buffer, "Unicode")
        skipOutBOM = False
    ElseIf buffer(bufferStart + 0) = 239 And buffer(bufferStart + 1) = 187 And buffer(bufferStart + 2) = 191 Then
        originalString = StringFromBytes(buffer, "UTF-8")
        skipOutBOM = False
    Else
        'If this file was actually a legacy encoding (most likely Windows-1252) this will work for the standard english characters, and fail for accented and other later characters. Too bad.
        originalString = StringFromBytes(buffer, "UTF-8")
        skipOutBOM = True
    End If
    
    Dim newString As String
    Dim FormattingManager As New PoorMansTSqlFormatterLib.SqlFormattingManager
    newString = FormattingManager.Format(originalString)
    
    buffer = BytesFromString(newString, "utf-8", skipOutBOM)

End Function


Public Function UnpackBufferA(ByRef buffer() As Byte, ByRef size As Long, ByRef bChanged As Boolean, ByRef subcode As Long) As Boolean
    
    ReformatByteBuffer buffer
    
    size = UBound(buffer)
    bChanged = True
    
    UnpackBufferA = True
End Function

Public Function PackBufferA(ByRef buffer() As Byte, ByRef size As Long, ByRef bChanged As Boolean, subcode As Long) As Boolean
    bChanged = False
    PackBufferA = False
End Function
Public Function UnpackFile(fileSrc As String, fileDst As String, ByRef bChanged As Boolean, ByRef subcode As Long) As Boolean
    Dim buffer() As Byte
    
    ReadByteArray fileSrc, buffer
    
    ReformatByteBuffer buffer
    
    WriteByteArray fileDst, buffer
    
    bChanged = True
    UnpackFile = True
End Function

Public Function PackFile(fileSrc As String, fileDst As String, ByRef bChanged As Boolean, subcode As Long) As Boolean
    ' Don't want to mess with repacking / saving modified files, because we might have lost some data in the case of encoding issues...
    bChanged = False
    PackFile = False 'report error!
    subcode = 1
End Function

