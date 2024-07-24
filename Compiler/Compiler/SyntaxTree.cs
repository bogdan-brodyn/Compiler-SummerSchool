// Copyright (c) 2024
//
// Use of this source code is governed by an MIT license
// that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

namespace Compiler;

public class SyntaxTree
{
    public SyntaxTree(Token rootToken, List<SyntaxTree>? children = null)
    {
        this.RootToken = rootToken;
        if (children == null)
        {
            this.Children = new List<SyntaxTree>();
        }
        else
        {
            this.Children = children;
        }
    }

    public SyntaxTree(Token rootToken, params SyntaxTree[] children)
    {
        this.RootToken = rootToken;
        this.Children = new List<SyntaxTree>(children);
    }

    public Token RootToken { get; set; }

    public List<SyntaxTree> Children { get; set; }
}
