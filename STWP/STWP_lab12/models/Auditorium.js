module.exports = (sequelize, DataTypes) => {
    const Auditorium = sequelize.define('Auditorium', {
        AUDITORIUM: {
            type: DataTypes.STRING(20),
            allowNull: false,
            primaryKey: true
        },
        AUDITORIUM_NAME: {
            type: DataTypes.STRING(50),
            allowNull: true // В схеме не указано NOT NULL
        },
        AUDITORIUM_CAPACITY: {
            type: DataTypes.INTEGER,
            allowNull: false,
            validate: { // Добавим валидацию, аналогичную CHECK в SQL
                min: 1
            }
        },
        AUDITORIUM_TYPE: {
            type: DataTypes.STRING(10),
            allowNull: false
            // Связь определяется в db.js
        }
    }, {
        tableName: 'AUDITORIUM',
        timestamps: false
    });
    return Auditorium;
};