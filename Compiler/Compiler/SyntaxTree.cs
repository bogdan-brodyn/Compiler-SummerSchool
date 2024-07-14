// Copyright (c) 2024
//
// Use of this source code is governed by an MIT license
// that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

namespace Compiler;

public class SyntaxTree
{
    private readonly Token rootToken;
    private readonly List<SyntaxTree> children;

    public SyntaxTree(Token rootToken, List<SyntaxTree>? children = null)
    {
        this.rootToken = rootToken;
        this.children = new List<SyntaxTree>();
        if (children != null)
        {
            this.children.AddRange(children);
        }
    }

    public SyntaxTree(Token rootToken, params SyntaxTree[] children)
    {
        this.rootToken = rootToken;
        this.children = new List<SyntaxTree>(children);
    }
}
