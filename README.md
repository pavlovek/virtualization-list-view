# VirtualizationListView
WPF UserControl realize virtualization ListView element supported changing, sorting and filtering.

The purpose of this library is to provide a convenient WPF UserControl for displaying mutable virtualized data with flexible sorting and filtering settings.
The library is built on the .NET Framework 4.5 in the Visual Studio 2017.
More about using virtualization data algorithm you can read on our article:
http://injoit.org/index.php/j1/article/view/485

# Features

* Free open source library
* Simple usage in your project in the XAML code
* Presentation of different types of objects in grid
* Localization capabilities
* Flexible filtering and sorting
* Fast filtering and complex filters constructor
* Simple handling list changes without loading the entire visible area

# How to use

* Add reference this libraries to your project
* Add the VirtualizationListView to your XAML code
* Add column descriptions for displaying your grid rows
* Realize the data source (on request from the database or using DynamicLinq or others)
* If necessary, configure the necessary filters for your data

**An example of use is in the source code**

# Dependencies

* System.Threading.Tasks.Dataflow
* System.Windows.Interactivity
