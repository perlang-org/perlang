#!/usr/bin/env ruby

# Used for camelize
require 'active_support'
require 'active_support/core_ext/string/inflections'

INDENT = ' ' * 4
VISIBILITY = 'public'

class Type
  attr_reader :class_name
  attr_reader :fields

  def initialize(class_name, *fields)
    @class_name = class_name
    @fields = fields.flatten
  end
end

class Field
  attr_reader :type
  attr_reader :name

  def initialize(type, name)
    @type = type
    @name = name
  end
end

def fix_stmt_name(name)
  # Hack since we had to rename Expression to ExpressionStmt, to avoid clashing with the
  # 'Expression' property. Without this, we get VisitExpressionStmtStmt instead of
  # VisitExpressionStmt
  name.sub('StmtStmt', 'Stmt')
end

def define_visitor(base_name, types)
  stream = StringIO.new

  stream.puts "#{INDENT * 2}#{VISIBILITY} interface IVisitor<TR>"
  stream.puts "#{INDENT * 2}{"

  types.each { |t|
    method_name = fix_stmt_name(t.class_name + base_name)

    stream.puts "#{INDENT * 3}TR Visit#{method_name}(#{t.class_name} #{base_name.downcase});"
  }

  stream.puts "#{INDENT * 2}}"

  stream.string
end

def define_type(base_name, class_name, fields)
  stream = StringIO.new

  stream.puts "#{INDENT * 2}#{VISIBILITY} class #{class_name} : #{base_name}"
  stream.puts "#{INDENT * 2}{"

  # Fields.
  fields.each { |f|
    stream.puts("#{INDENT * 3}#{VISIBILITY} #{f.type} #{f.name.camelize} { get; }");
  }

  stream.puts

  # Constructor.
  constructor_fields = fields.map { |f|
    "#{f.type} #{f.name}"
  }.join(', ')
  stream.puts "#{INDENT * 3}#{VISIBILITY} #{class_name}(#{constructor_fields}) {"

  # Store parameters in fields.
  fields.each { |f|
    stream.puts "#{INDENT * 4}#{f.name.camelize} = #{f.name};"
  }

  stream.puts "#{INDENT * 3}}";

  stream.puts

  # Visitor pattern implementation
  method_name = fix_stmt_name(class_name + base_name)
  stream.puts "#{INDENT * 3}#{VISIBILITY} override TR Accept<TR>(IVisitor<TR> visitor)"
  stream.puts "#{INDENT * 3}{"
  stream.puts "#{INDENT * 4}return visitor.Visit#{method_name}(this);"
  stream.puts "#{INDENT * 3}}"

  stream.puts "#{INDENT * 2}}"

  stream
end

def define_ast(output_dir, base_name, types)
  visitor_content = define_visitor(base_name, types)

  inner_classes = types.map { |type|
    define_type(base_name, type.class_name, type.fields)
  }

  inner_classes_content = inner_classes
    .map(&:string)
    .join("\n")
    .rstrip

  path = File.join(output_dir, base_name + ".cs");
  File.write(path, <<~EOF)
//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//
// Instead, change the #{$0} script that generated this code.
//
using System.Collections.Generic;

namespace Perlang
{
#{INDENT}#{VISIBILITY} abstract class #{base_name}
#{INDENT}{
#{visitor_content}
#{inner_classes_content}

#{INDENT * 2}#{VISIBILITY} abstract TR Accept<TR>(IVisitor<TR> visitor);
#{INDENT}}
}
  EOF
end

OUTPUT_DIR = 'Perlang.Common'

#
# Certain names like _params and _operator are prefixed, since they
# clash with C# reserved words.
#

# Expressions
define_ast(OUTPUT_DIR, "Expr", [
  Type.new('Empty'),
  Type.new('Assign', Field.new('Token', 'name'), Field.new('Expr', 'value')),
  Type.new('Binary', [
    Field.new('Expr', 'left'),
    Field.new('Token', '_operator'),
    Field.new('Expr', 'right')
  ]),
  Type.new('Call', [
    Field.new('Expr', 'callee'),
    Field.new('Token', 'paren'),
    Field.new('List<Expr>', 'arguments')
  ]),
  Type.new('Grouping', Field.new('Expr', 'expression')),
  Type.new('Literal', Field.new('object', 'value')),
  Type.new('Logical', [
    Field.new('Expr', 'left'),
    Field.new('Token', '_operator'),
    Field.new('Expr', 'right')
  ]),
  Type.new('UnaryPrefix', [
    Field.new('Token', '_operator'),
    Field.new('Expr', 'right')
  ]),
  Type.new('UnaryPostfix', [
    Field.new('Expr', 'left'),
    Field.new('Token', 'name'),
    Field.new('Token', '_operator')
  ]),
  Type.new('Variable', Field.new('Token', 'name'))
])

# Statements
define_ast(OUTPUT_DIR, "Stmt", [
  Type.new('Block', Field.new('List<Stmt>', 'statements')),

  # Silly name to avoid clash with the 'Expression' property that
  # gets generated for the 'expression' field.
  Type.new('ExpressionStmt', Field.new('Expr', 'expression')),

  Type.new('Function', [
    Field.new('Token', 'name'),
    Field.new('List<Token>', '_params'),
    Field.new('List<Stmt>', 'body')
  ]),
  Type.new('If', [
    Field.new('Expr', 'condition'),
    Field.new('Stmt', 'thenBranch'),
    Field.new('Stmt', 'elseBranch')
  ]),
  Type.new('Print', Field.new('Expr', 'expression')),
  Type.new('Return', Field.new('Token', 'keyword'), Field.new('Expr', 'value')),
  Type.new('Var', [
    Field.new('Token', 'name'),
    Field.new('Expr', 'initializer')
  ]),
  Type.new('While', Field.new('Expr', 'condition'), Field.new('Stmt', 'body'))
])
