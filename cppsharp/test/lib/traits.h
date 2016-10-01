#ifndef TRAITS_H
#define TRAITS_H

struct TrueType { static const bool value = true; };
struct FalseType { static const bool value = false; };

template<class T, class U>
struct SameType : FalseType
{};

template<class T>
struct SameType<T, T> : TrueType
{};

#endif // TRAITS_H
