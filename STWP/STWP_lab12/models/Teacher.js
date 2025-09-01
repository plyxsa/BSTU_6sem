module.exports = (sequelize, DataTypes) => {
    const Teacher = sequelize.define('Teacher', {
        TEACHER: {
            type: DataTypes.STRING(20),
            allowNull: false,
            primaryKey: true
        },
        TEACHER_NAME: {
            type: DataTypes.STRING(100),
            allowNull: false
        },
        PULPIT: {
            type: DataTypes.STRING(20),
            allowNull: false
        }
    }, {
        tableName: 'TEACHER',
        timestamps: false
    });
    return Teacher;
};