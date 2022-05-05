const tool={};
tool.copy = function (json){
    return JSON.parse(JSON.stringify(json));
}
/**
 * element 3.0 日期时间控件返回值无法自动转换为当前时区
 * @param date 能转为date类型的字符串
 * @returns {number} 时间戳
 * @constructor
 */
tool.GMT = function (date){
    let d = new Date(date)
    return d.getTime()
}
export default tool;