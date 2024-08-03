// Copyright (c) 2024
//
// Use of this source code is governed by an MIT license
// that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

namespace Compiler;

public class CompilerException(int line, string message)
    : SystemException($"Line ({line})\n{message}")
{
}
