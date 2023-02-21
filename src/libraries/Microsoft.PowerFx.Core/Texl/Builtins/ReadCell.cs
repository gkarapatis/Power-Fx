// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.PowerFx.Core.App.ErrorContainers;
using Microsoft.PowerFx.Core.Binding;
using Microsoft.PowerFx.Core.Functions;
using Microsoft.PowerFx.Core.IR;
using Microsoft.PowerFx.Core.Localization;
using Microsoft.PowerFx.Core.Types;
using Microsoft.PowerFx.Core.Utils;
using Microsoft.PowerFx.Syntax;
using Microsoft.PowerFx.Types;

namespace Microsoft.PowerFx.Core.Texl.Builtins
{
    // ReadCell(source:*, n, n)
    internal sealed class ReadCell : FunctionWithTableInput
    {
        public delegate FormulaValue RuntimeExpressionResolver(string expression);

        public static RuntimeExpressionResolver EvalExpression;

        public override bool IsSelfContained => true;

        public override bool SupportsParamCoercion => false;

        public ReadCell()
            : base(
                  "ReadCell",
                  TexlStrings.AboutIndex,
                  FunctionCategories.Table,
                  DType.Unknown,
                  0,
                  3,
                  3,
                  DType.EmptyTable,
                  DType.Number,
                  DType.Number)
        {
        }

        public static FormulaValue ReadCell1(IRContext irContext, FormulaValue[] args)
        {
            var arg0 = (TableValue)args[0];
            var arg1 = (NumberValue)args[1];
            var rowIndex = (int)arg1.Value;

            return arg0.Index(rowIndex).ToFormulaValue();
        }

        public override IEnumerable<TexlStrings.StringGetter[]> GetSignatures()
        {
            yield return new[] { TexlStrings.IndexArg1, TexlStrings.IndexArg2 };
        }

        public override bool CheckTypes(CheckTypesContext context, TexlNode[] args, DType[] argTypes, IErrorContainer errors, out DType returnType, out Dictionary<TexlNode, DType> nodeToCoercedTypeMap)
        {
            var fArgsValid = base.CheckTypes(context, args, argTypes, errors, out returnType, out nodeToCoercedTypeMap);
            if (args.Length > 2)
            {
                object argValue = string.Empty;
                switch (args[2])
                {
                    case NumLitNode num1:
                        argValue = num1.Value.Value;
                        break;
                    case StrLitNode str1:
                        argValue = str1.Value;
                        break;
                    default:
                        var result = EvalExpression != null ? EvalExpression(args[2].ToString()) : null;
                        if (result is NumberValue numValue)
                        {
                            argValue = numValue.Value;
                        }
                        else if (result is NumberValue strValue)
                        {
                            argValue = strValue.Value;
                        }

                        break;
                }

                var input = argTypes[0].ToRecord();
                if (argValue is double num)
                {
                    var fields = input.GetRootFieldNames();
                    if (fields.Count() > num)
                    {
                        input.TryGetType(fields.ElementAt(Convert.ToInt32(num)), out returnType);
                    }
                }
                else if (argValue is string str && !string.IsNullOrEmpty(str))
                {
                    input.TryGetType(new DName(str), out returnType);
                }
            }

            returnType = returnType ?? DType.Unknown;

            return fArgsValid;
        }
    }
}
