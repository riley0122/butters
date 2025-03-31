# Butters

The butters programming language is a language that is made in c#, its runtime and compiler are highly customisable since it first outputs to a json like structure to then be further compiled or ran

# usage

to compile a butters file (.btrs) use the command:

```
Butters comp file.btrs
```

to run a compiled file (.bcomp):

```
Butters run file.bcomp
```

to do both after eachother:

```
Butters do file.btrs
```

# Structure

[view guide](https://github.com/lucasammer/butters/blob/main/guides/guide.btrs)

All butters programs are structured in sections.
Not all sections are always used.
All possible sections are:

- META
- STATIC
- DYNAMIC
- DEFINE
- CODE

Sections are declared as such:

```butters
#section [name]
```

### META

This defines the metadata of the file.

To add metadata you can add one of these in the section:

- author: the author of the file
- project: the project name
- version: the current version
- license: the license

These are added prefixed with a \*.

like this (not all data is required):

```butters
#section META
*author [name]
*project [name]
```

### STATIC & DYNAMIC

These are the import functions in Butters.

Static imports are not yet implemented!

Dynamic imports can be used like this:
```butters
#section DYNAMIC
import greet
```

This will import the file `greet.btrs`.

> **Note**
> The imported file name must have the same file name as project name.
> 
> For example `greet.btrs` must have `*project greet` in the META section.

### DEFINE

All variables will be defined here, they _can_ be changed later on.

### CODE

This is where all your code is sittuated.
