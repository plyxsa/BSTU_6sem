var util = require('util');
var ee = require('events');

var db_data = [
    { id: 1, name: 'Иванов И.И.', bday: '2001-01-01' },
    { id: 2, name: 'Петров П.П.', bday: '2001-01-02' },
    { id: 3, name: 'Сидоров С.С.', bday: '2001-01-03' }
];

function DB() {
    this.requestsCount = 0;
    this.commitsCount = 0;

    this.select = () => {
        this.requestsCount++;
        return db_data;
    };

    this.insert = (record) => {
        this.requestsCount++;
        db_data.push(record);
    };

    this.update = (record) => {
        this.requestsCount++;
        let index = db_data.findIndex(item => item.id === record.id);
        if (index !== -1) {
            db_data[index] = record;
            return record;
        }
        return null;
    };

    this.delete = (id) => {
        this.requestsCount++;
        let index = db_data.findIndex(item => item.id === id);
        if (index !== -1) {
            return db_data.splice(index, 1)[0];
        }
        return null;
    };

    this.commit = () => {
        this.commitsCount++;
        console.log(`[COMMIT] Состояние БД зафиксировано.`);
    };
}

util.inherits(DB, ee.EventEmitter);
exports.DB = DB;
