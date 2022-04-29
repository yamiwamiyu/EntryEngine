const storage = {
    // token
    tokenSet: function (val) {
        localStorage.setItem('token', val);
    },
    tokenGet: function () {
        return localStorage.getItem('token');
    },

    // services
    serviceAdd: function (val) {
        if (val) {
            let services = localStorage.getItem('services');
            console.log('storage', services);
            let list = [];
            if (services) {
                list = JSON.parse(services)
                let i = list.indexOf(val);
                if (i != -1) {
                    list.splice(i, 1)
                }
            }
            list.push(val)
            console.log('serviceAdd', list);
            localStorage.setItem('services', JSON.stringify(list));
        }
    },
    serviceDel: function (index) {
        let services = localStorage.getItem('services');
        let list = [];
        if (services) {
            list = JSON.parse(services)
        }
        list.splice(index, 1)
        localStorage.setItem('services', JSON.stringify(list));
        return list
    },
    serviceGet: function () {
        let services = localStorage.getItem('services');
        let list = [];
        if (services) {
            list = JSON.parse(services)
        }
        console.log('serviceGet', list);
        return list
    },

    // user
    userAdd: function (val) {
        if (val) {
            let user = localStorage.getItem('user');
            let list = [];
            if (user) {
                list = JSON.parse(user)
                let i = list.indexOf(val);
                if (i != -1) {
                    list.splice(i, 1)
                }
            }
            list.push(val)
            localStorage.setItem('user', JSON.stringify(list));
        }
    },
    userDel: function (index) {
        let user = localStorage.getItem('user');
        let list = [];
        if (user) {
            list = JSON.parse(user)
        }
        list.splice(index, 1)
        localStorage.setItem('user', JSON.stringify(list));
        return list
    },
    userGet: function () {
        let user = localStorage.getItem('user');
        let list = [];
        if (user) {
            list = JSON.parse(user)
        }
        console.log('userGet', list);
        return list
    },

    // 服务列表,最后一次版本筛选
    serviceTypeSet: function (val) {
        localStorage.setItem('serviceType', val);
    },
    serviceTypeGet: function () {
        return localStorage.getItem('serviceType');
    },
}

export default storage