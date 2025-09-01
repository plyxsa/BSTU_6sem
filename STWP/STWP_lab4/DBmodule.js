var util = require('util');    
var ee = require('events');    

var db_data = [
    { id: 1, name: 'Иванов И.И.', bday: '2001-01-01' },
    { id: 2, name: 'Петров П.П.', bday: '2001-01-02' },
    { id: 3, name: 'Сидоров С.С.', bday: '2001-01-03' }
];

function DB() {
    this.select = () => db_data;    

    this.insert = (record) => {
        db_data.push(record);
    };

    this.update = (record) => {
        let index = db_data.findIndex(item => item.id === record.id);
        if (index !== -1) {
            db_data[index] = record;
            return record;
        }
        return null;
    };

    this.delete = (id) => {
        let index = db_data.findIndex(item => item.id === id);
        if (index !== -1) {
            return db_data.splice(index, 1)[0];
        }
        return null;
    };
}

util.inherits(DB, ee.EventEmitter);    

exports.DB = DB;
