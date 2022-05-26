// 查找

// 查找符合条件的第一个元素，没有符合的返回undefined
// Boolean func(value)，func可不传，此时返回第一个元素
Object.prototype._First = function(func) {
    for (var value of this)
        if (!func || func(value))
            return value;
}
// 查找符合条件的最后一个元素，没有符合的返回undefined
// Boolean func(value)，func可不传，此时返回最后一个元素
Object.prototype._Last = function(func) {
    if (!func && this instanceof Array) {
        if (this.length == 0)
            return undefined;
        else
            return this[this.length - 1];
    } else {
        var last;
        for (var value of this)
            if (!func || func(value))
                last = value;
        return last;
    }
}
// 查找符合条件的所有元素，没有符合的返回undefined
// Boolean func(value)
Object.prototype._Where = function*(func) { 
    for (var value of this) 
        if (func(value))
            yield value;
}
// 数组所有元素均为true时则为true，空数组为true
// Boolean func(value)
Object.prototype._All = function(func) { 
	for (var value of this)
		if (!func(value))
			return false;
	return true;
}
// 数组任意元素为true时则为true，空数组为false
// Boolean func(value)
Object.prototype._Any = function(func) { 
	for (var value of this)
		if (func(value))
			return true;
	return false;
}


// 转换迭代器类型

// 选择迭代器元素中的某个字段作为新的迭代器
// Any func(value)
Object.prototype._Select = function*(func) { 
    for (var value of this) 
        yield func(value);
}
// 联合迭代器元素中的某个迭代器字段作为新的迭代器
// Any func(value)
Object.prototype._SelectMany = function*(func) {
    for (var value of this) {
        var many = func(value);
        if (many)
            for (var value2 of many)
                yield value2;
    }
}
// 将有相同键的元素分组，相当于字典，返回Array[key, Array[]]
// Any funcKey(value)
// Any funcValue(value)，funcValue可不传，值就是原始迭代器的元素
Object.prototype._GroupBy = function(funcKey, funcValue) {
    var group = [];
    var key;
    for (var value of this) {
        key = funcKey(value);
        if (!group[key])
            group[key] = [];
        if (funcValue)
            group[key].push(funcValue(value));
        else
            group[key].push(value);
    }
    return group;
}
// 将迭代器转换成数组，若原本是数组，也会返回一个新数组
Object.prototype._ToArray = function() {
    var array = [];
    for (var value of this)
        array.push(value);
    return array;
}

// 聚合函数

// 返回满足条件的元素个数
// Boolean func(value)，func可不传，此时返回迭代器的元素数量
Object.prototype._Count = function(func) {
    if (!func && this instanceof Array)
        return this.length;
    var count = 0;
    for (var value of this)
        if (!func || func(value))
            count++;
    return count;
}
// 计算数组所有元素的平均值
// Number func(value)，func可不传，此时this必须是Number类型的迭代器
Object.prototype._Avg = function(func) {
    var sum = 0;
    var count = 0;
    if (func) {
	    for (var value of this) {
            sum += func(value);
            count++;
        }
    } else {
        for (var value of this) {
            sum += value;
            count++;
        }
    }
    if (count)
        return sum / count;
    else
        return 0;
}
// 计算数组所有元素的累加值
// Number func(value)，func可不传，此时this必须是Number类型的迭代器
Object.prototype._Sum = function(func) {
    var sum = 0;
    if (func)
	    for (var value of this)
            sum += func(value);
    else
        for (var value of this)
            sum += value;
    return sum;
}
// 查找最大值或最大值相应的元素
// Number func(value)，func可不传，此时this必须是Number类型的迭代器
// e，默认为true，为true时返回元素，否则返回数值
Object.prototype._Max = function(func, e = true) {
    // 最大值对应的对象
    var result;
    if (func) {
        // 最大值
        var rvalue;
        var temp;
        for (var value of this) {
            temp = func(value);
            if (rvalue == undefined || temp > rvalue) {
                rvalue = temp;
                result = value;
            }
        }
        if (!e)
            result = rvalue;
    } else {
        for (var value of this) {
            if (result == undefined || value > result)
                result = value;
        }
    }
    return result;
}
// 查找最小值或最小值相应的元素
// Number func(value)，func可不传，此时this必须是Number类型的迭代器
// e，默认为true，为true时返回元素，否则返回数值
Object.prototype._Min = function(func, e = true) {
    // 最小值对应的对象
    var result;
    if (func) {
        // 最大值
        var rvalue;
        var temp;
        for (var value of this) {
            temp = func(value);
            if (rvalue == undefined || temp < rvalue) {
                rvalue = temp;
                result = value;
            }
        }
        if (!e)
            result = rvalue;
    } else {
        for (var value of this) {
            if (result == undefined || value < result)
                result = value;
        }
    }
    return result;
}


// 其它

// 合并另一个迭代器
Object.prototype._Concat = function*(iterator) {
    for (var value of this)
        yield value;
    for (var value of iterator)
        yield value;
}
// 跳过序列中指定数量的元素，然后返回剩余的元素
Object.prototype._Skip = function*(count) {
    if (count <= 0)
        return this;
    if (this instanceof Array) {
        for (var i = count; i < this.length; i++)
            yield this[i];
    } else {
        for (var value of this) {
            count--;
            if (count < 0)
                yield value;
        }
    }
}
// 从序列的开头返回指定数量的连续元素
Object.prototype._Take = function*(count) {
    for (var value of this) {
        count--;
        if (count >= 0)
            yield value;
        else
            break;
    }
}
// 只要满足指定的条件，就跳过序列中的元素，然后返回剩余元素
// Boolean func(value)
Object.prototype._SkipWhile = function*(func) {
    var skip = true;
    for (var value of this) {
        if (skip) {
            skip = func(value);
            if (skip)
                continue;
        }
        yield value;
    }
}
// 只要满足指定的条件，就会返回序列的元素
// Boolean func(value)
Object.prototype._TakeWhile = function*(func) {
    for (var value of this) {
        if (func(value))
            yield value;
        else
            break;
    }
}