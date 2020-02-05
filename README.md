# namedpipes-wpf-c-sharp
Simple solution, with two C# WPF project applications, providing an example of inter-app duplex asynchronous communication.

Utilising named pipes allows communication between two related or non-related processes, utilising per-instance buffers and handles.

I've provided this solution on GitHub, as found other examples to vary, and not actually provide what I wanted (two applications being able to message each other at will, keeping both alive indefinitely, and then being able to close either, which in turn closes the other).
