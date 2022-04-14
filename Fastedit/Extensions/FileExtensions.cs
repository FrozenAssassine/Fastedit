using System.Collections.Generic;

namespace Fastedit.Extensions
{
    public class FileExtensions
    {
        public List<ExtensionList> FileExtentionList = new List<ExtensionList>();

        public FileExtensions()
        {
            FileExtentionList.Add(Batch);   //.bat
            FileExtentionList.Add(C);       //.c
            FileExtentionList.Add(CPP);     //.cpp
            FileExtentionList.Add(CSharp);  //.cs
            FileExtentionList.Add(CSS);     //.css
            FileExtentionList.Add(HTML);    //.html
            FileExtentionList.Add(INI);     //.ini
            FileExtentionList.Add(Java);    //.java
            FileExtentionList.Add(Python);  //.py
            FileExtentionList.Add(Reg);     //.reg
            FileExtentionList.Add(Text);    //.txt
            FileExtentionList.Add(XAML);    //.xaml
            FileExtentionList.Add(XML);     //.xml
            FileExtentionList.Add(Json);     //.json
            FileExtentionList.Add(Gcode);     //.gcode
            FileExtentionList.Add(VisualBasic);     //.vb
            FileExtentionList.Add(ArduinoSketch);     //.ino
            FileExtentionList.Add(PHP);     //.php
            FileExtentionList.Add(Assembly);     //.asm
            FileExtentionList.Add(Kotlin);     //.kt
            FileExtentionList.Add(Markdown);     //.md

            //Sort list aplhabatically
            FileExtentionList.Sort((a, b) => a.ExtensionName.CompareTo(b.ExtensionName));
        }
        public ExtensionList Markdown = new ExtensionList()
        {
            Extension = { ".md", ".markdown", ".mdown", ".markdn" },
            ExtensionName = "Markdown",
            ExtensionLongName = "Markdown"
        };
        public ExtensionList Json = new ExtensionList()
        {
            Extension = { ".json" },
            ExtensionName = "Json",
            ExtensionLongName = "Json"
        };
        public ExtensionList Gcode = new ExtensionList()
        {
            Extension = { ".gcode", ".ngc", ".tap" },
            ExtensionName = "G-Code",
            ExtensionLongName = "G-Code"
        };
        public ExtensionList VisualBasic = new ExtensionList()
        {
            Extension = { ".vb" },
            ExtensionName = "Visual Basic",
        };
        public ExtensionList ArduinoSketch = new ExtensionList()
        {
            Extension = { ".ino" },
            ExtensionName = "Arduino sketch",
        };
        public ExtensionList PHP = new ExtensionList()
        {
            Extension = { ".php" },
            ExtensionName = "Hypertext preprocessor",
        };
        public ExtensionList Assembly = new ExtensionList()
        {
            Extension = { ".asm" },
            ExtensionName = "Assembly language",
        };
        public ExtensionList Kotlin = new ExtensionList()
        {
            Extension = { ".kt" },
            ExtensionName = "Kotlin",
        };
        public ExtensionList CSharp = new ExtensionList()
        {
            Extension = { ".cs" },
            ExtensionName = "C-Sharp"
        };
        public ExtensionList CPP = new ExtensionList()
        {
            Extension = { ".cpp", ".cxx", ".cc", ".hpp" },
            ExtensionName = "C++"
        };
        public ExtensionList Python = new ExtensionList()
        {
            Extension = { ".py", ".py3", ".pyt", ".rpy", ".pyw" },
            ExtensionName = "Python",
            ExtensionLongName = "Python"
        };
        public ExtensionList Batch = new ExtensionList()
        {
            Extension = { ".bat" },
            ExtensionName = "Batch",
            ExtensionLongName = "Windows batch"
        };
        public ExtensionList XAML = new ExtensionList()
        {
            Extension = { ".xaml" },
            ExtensionName = "Xaml",
            ExtensionLongName = "Extensible Application Markup Language"
        };
        public ExtensionList XML = new ExtensionList()
        {
            Extension = { ".xml" },
            ExtensionName = "XML",
            ExtensionLongName = "Extensible Markup Language"
        };
        public ExtensionList HTML = new ExtensionList()
        {
            Extension = { ".html", ".htm", ".xhtml" },
            ExtensionName = "HTML",
            ExtensionLongName = "Hypertext Markup Language"
        };
        public ExtensionList Text = new ExtensionList()
        {
            Extension = { ".txt", ".log" },
            ExtensionName = "Textfile",
            ExtensionLongName = "Textfile"
        };
        public ExtensionList Reg = new ExtensionList()
        {
            Extension = { ".reg" },
            ExtensionName = "Registration file",
            ExtensionLongName = "Windows Registration file"
        };
        public ExtensionList CSS = new ExtensionList()
        {
            Extension = { ".css" },
            ExtensionName = "Cascading Style Sheets",
            ExtensionLongName = "Cascading Style Sheets"
        };
        public ExtensionList Java = new ExtensionList()
        {
            Extension = { ".java", ".jav", ".j" },
            ExtensionName = "Java",
            ExtensionLongName = "Java"
        };
        public ExtensionList INI = new ExtensionList()
        {
            Extension = { ".ini" },
            ExtensionName = "Configuration file",
            ExtensionLongName = "Configuration file"
        };
        public ExtensionList C = new ExtensionList()
        {
            Extension = { ".c", ".h" },
            ExtensionName = "C",
            ExtensionLongName = "C"
        };
    }

    public class ExtensionList
    {
        public List<string> Extension = new List<string>();
        public string ExtensionName { get; set; }
        public string ExtensionLongName { get; set; }
    }
}