﻿//  Copyright 2010, 2011 by Konstantin Ulitin
//            2012 by Deikin Alexander <eskendirrr@gmail.com>
//
//  This file is part of YaccConctructor.
//
//  YaccConstructor is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace Yard.Core

open Yard.Core.IL

[<AbstractClass>]
type Frontend() as this = 
    abstract Name : string
    //abstract CheckGrammar : Definition.t<Source.t,Source.t> -> bool
    abstract ParseGrammar : obj -> Definition.t<Source.t,Source.t>
    //member public self.ParseGrammar = parse_grammar
    abstract ProductionTypes : string list
    interface Yard.Core.Manager.IComponent with
        member self.Name : string = this.Name