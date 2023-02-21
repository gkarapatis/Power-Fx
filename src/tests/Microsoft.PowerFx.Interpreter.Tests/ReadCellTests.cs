// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.PowerFx.Core.App.ErrorContainers;
using Microsoft.PowerFx.Core.Binding;
using Microsoft.PowerFx.Core.Errors;
using Microsoft.PowerFx.Core.Functions;
using Microsoft.PowerFx.Core.Functions.FunctionArgValidators;
using Microsoft.PowerFx.Core.IR;
using Microsoft.PowerFx.Core.Localization;
using Microsoft.PowerFx.Core.Tests;
using Microsoft.PowerFx.Core.Texl.Builtins;
using Microsoft.PowerFx.Core.Types;
using Microsoft.PowerFx.Core.Utils;
using Microsoft.PowerFx.Interpreter;
using Microsoft.PowerFx.Syntax;
using Microsoft.PowerFx.Types;
using Xunit;
using static Microsoft.PowerFx.Core.Localization.TexlStrings;

namespace Microsoft.PowerFx.Interpreter.Tests
{
    public class ReadCellTests : PowerFxTest
    {
        [Fact]
        public void ReadCellCheck()
        {
            var engine = new Engine(new PowerFxConfig(CultureInfo.InvariantCulture));
            var r1 = FormulaValue.NewRecordFromFields(new NamedValue("p1", FormulaValue.New(1)), new NamedValue("p2", FormulaValue.New("v")));
            var r2 = FormulaValue.NewRecordFromFields(new NamedValue("p1", FormulaValue.New(2)), new NamedValue("p2", FormulaValue.New("k")));
            var t1 = FormulaValue.NewTable(r1.Type, r1, r2);
            var x = FormulaValue.New(0);
            var y = FormulaValue.New(1);
            var scope = FormulaValue.NewRecordFromFields(new NamedValue("var", t1), new NamedValue("x", x), new NamedValue("y", y));
            var checkresult0 = engine.Check("ReadCell(var, 1, 0)", scope.Type);
            var checkresult1 = engine.Check("ReadCell(var, 1, 1)", scope.Type);

            //var checkresult3 = engine.Check("ReadCell(var, 1, \"p1\")", scope.Type);
            //var checkresult4 = engine.Check("ReadCell(var, 1, \"p2\")", scope.Type);

            var checkresult5 = engine.Check("ReadCell(var, 1, x)", scope.Type);

            Assert.IsType<NumberType>(checkresult0.ReturnType);
            Assert.IsType<StringType>(checkresult1.ReturnType);

            //Assert.IsType<NumberType>(checkresult3.ReturnType);
            //Assert.IsType<StringType>(checkresult4.ReturnType);

            Assert.IsType<UnknownType>(checkresult5.ReturnType);
        }

        [Fact]
        public void ReadCellEval()
        {
            try
            {
                var engine = new Engine(new PowerFxConfig(CultureInfo.InvariantCulture));
                var r1 = FormulaValue.NewRecordFromFields(new NamedValue("p1", FormulaValue.New(1)), new NamedValue("p2", FormulaValue.New("v")));
                var r2 = FormulaValue.NewRecordFromFields(new NamedValue("p1", FormulaValue.New(2)), new NamedValue("p2", FormulaValue.New("k")));
                var t1 = FormulaValue.NewTable(r1.Type, r1, r2);
                var x = FormulaValue.New(0);
                var y = FormulaValue.New(1);
                var scope = FormulaValue.NewRecordFromFields(new NamedValue("var", t1), new NamedValue("x", x), new NamedValue("y", y));

                var recalcEngine = new Microsoft.PowerFx.RecalcEngine();

                ReadCell.EvalExpression = (x) => recalcEngine.Eval(x, scope);

                var runresult0 = recalcEngine.Eval("ReadCell(var, 1, x)", scope);
                var runresult1 = recalcEngine.Eval("ReadCell(var, 1, x + 1)", scope);

                Assert.Equal(2, ((NumberValue)runresult0).Value);
                Assert.Equal("k", ((StringValue)runresult1).Value);
            }
            finally
            {
                ReadCell.EvalExpression = null;
            }
        }
    }
}
