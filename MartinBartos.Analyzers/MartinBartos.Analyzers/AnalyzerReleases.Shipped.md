## Release 1.0

### New Rules

| Rule ID | Category   | Severity | Notes                                                              |
|---------|------------|----------|--------------------------------------------------------------------|
| MB1000  | Formatting | Warning  | [FileScopedNamespaceAnalyzer](#mb1000-filescopednamespaceanalyzer) |
| MB1010  | Formatting | Warning  | [MultipleLinesAnalyzer](#mb1010-multiplelinesanalyzer)
| MB1020  | Formatting | Warning  | [WrongIfFormatAnalyzer](#mb1020-wrongifformatanalyzer)

### MB1000 FileScopedNamespaceAnalyzer
Analyzes missing new line between File scope declaration and type declaration. Warning is added for following code:

```Csharp
namespace NamespaceName;
class ClassName { }
```

Code valid for this analyzer should look like this:
```Csharp
namespace NamespaceName;

class ClassName { }
```

### MB1010 MultipleLinesAnalyzer
Analyzes duplicit new lines, for example the following code is not valid:
```Csharp
Console.WriteLine("There should be only one empty line between.")


Console.WriteLine("But there are two.")
```

### MB1020 WrongIfFormatAnalyzer

Analyzis space between `if` and parentheses, for the example the following code is not valid:
```Csharp
if(false) 
{
}
```

The following code is valid:
```Csharp
if (false) 
{
}
```