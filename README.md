## Compiler for Summer School 2024

![ci-status](https://github.com/bogdan-brodyn/Compiler-SummerSchool/actions/workflows/ci.yml/badge.svg?event=push)

Compiler of simple imperative language for RISC-V architecture, implemented via .NET framework.
Generates RISC-V assembly code from given source code file

### Requirements:
  - .NET SDK version: 8.0.100
  - Microsoft.NETCore.App version: 8.0.0

### Build and Run:
1) Use following terminal commands to build the project:
```
git clone https://github.com/bogdan-brodyn/Compiler-SummerSchool.git
cd Compiler-SummerSchool/Compiler/Compiler
dotnet build
```

2) To compile your program in given language use:
``` dotnet run {source} {destination} ```

Where {source} is path of the source file, {destination} is path at which the assembly file will be created

## Language
Simple language features three types of statements: assignment, conditional operator "if" and a loop operator "while";
it has only one type, which is integer 

Language supports four basic arithmetic operations:
  - addition "+"
  - substraction "-"
  - multiplication "*"
  - division "/"

Expressions with these operations support parentheses and can contain variables as well as const values

### 1) Assignment
You can assign a value to the variable with following syntax:

```
{var} := {expression};
```

Example:
``` z := (x + y) / 32; ```

### 2) If
Conditional operator has following syntax:
```
if ({expression}) then
  {statements}
else
  {statements}
fi
```

Where "then" statements will be executed if value of the expression is positive, and "else" statements will be executed otherwise.
"else" keyword with respective statements can also be ignored

### 3) While
You can use following syntax to make a loop:
```
while ({expression}) do
  {statements}
done
```

Where "do" statements will be sequentially executed while value of the expression is positive

For example, here is how you could write a factorial:
```
acc:=1; n:=6;
while (n) do
    acc:=acc*n;
    n:=n-1;
done
```
"acc" variable will contain the factorial of 6 after execution
