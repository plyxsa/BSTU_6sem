module.exports = (sequelize, DataTypes) => {
    const Pulpit = sequelize.define('Pulpit', {
        PULPIT: {
            type: DataTypes.STRING(20),
            allowNull: false,
            primaryKey: true
        },
        PULPIT_NAME: {
            type: DataTypes.STRING(100),
            allowNull: false
        },
        FACULTY: {
            type: DataTypes.STRING(10),
            allowNull: false
        }
    }, {
        tableName: 'PULPIT',
        timestamps: false
    });
    return Pulpit;
};